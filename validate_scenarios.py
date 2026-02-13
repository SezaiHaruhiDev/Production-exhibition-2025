
import os
import re

def validate_scenario(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    errors = []
    labels = set()
    jumps = []

    # First pass: collect labels
    for i, line in enumerate(lines):
        line = line.strip()
        if not line: continue
        if line.startswith('#'):
            # #Label-Text
            parts = line[1:].split('-', 1)
            label = parts[0].strip()
            labels.add(label)

    # Second pass: check syntax
    current_line_of_page = False
    for i, line in enumerate(lines):
        raw_line = line
        line = line.strip()
        if not line: continue
        if line.startswith('#'):
            continue

        if not line.startswith('&'):
            # The parser splits by '&'. If a line doesn't start with '&', 
            # it becomes part of the previous '&' block.
            # This is technically allowed by the parser but might be a mistake.
            # Especially if it starts with '!' but no '&'.
            if line.startswith('!'):
                errors.append(f"Line {i+1}: Command line missing leading '&': {line}")
            elif '「' in line and not any(l.strip().startswith('&') for l in lines[max(0, i-5):i]):
                # Heuristic: if it has dialogue but no '&' nearby
                errors.append(f"Line {i+1}: Dialogue line missing leading '&': {line}")
        
        if line.startswith('&'):
            content = line[1:].strip()
            if not content: continue

            if content.startswith('!'):
                # Command(s)
                cmds = content[1:].split('!')
                for cmd_str in cmds:
                    if not cmd_str.strip(): continue
                    # Check for jumps in commands
                    # ch=1:text^label
                    # skip=label:label
                    # ck=ids:...,label:label
                    if 'label:' in cmd_str:
                        m = re.search(r'label:([^,^!&]+)', cmd_str)
                        if m: jumps.append((i+1, m.group(1).strip()))
                    if '^' in cmd_str:
                        # Split by comma first to avoid matching across params
                        parts = cmd_str.split(',')
                        for p in parts:
                            if '^' in p:
                                target = p.split('^')[-1].strip()
                                jumps.append((i+1, target))
            else:
                # Text page
                if '「' in content:
                    if '」' not in content:
                        errors.append(f"Line {i+1}: Missing closing bracket '」': {line}")
                    # Check if it has a name or is narration
                    # ScenarioParser.cs splits by '「'
                    # name = ts[0].Trim()
                elif '」' in content:
                    errors.append(f"Line {i+1}: Closing bracket '」' without opening: {line}")

    # Third pass: check jumps
    for line_num, target in jumps:
        # Ignore special logic labels if any (none obvious from parser)
        if target and target not in labels:
            # Check if it might be a special value like '0' or 'reset' or an asset name
            # From ScenarioParser, we don't know which params are jumps except by pattern
            # But ^label and label:label are common
            if target not in ['0', 'reset', 'true', 'false', 'p', 'impact']:
                # Some are probably SE or BG names, let's be careful.
                # In `!ch=...^label`, it's definitely a label.
                # In `!skip=label:label`, it's definitely a label.
                errors.append(f"Line {line_num}: Jump target '{target}' not found in labels.")

    return errors

texts_dir = '/Users/haruhisezai/Production-exhibition-2025/Assets/Resources/texts'
txt_files = [f for f in os.listdir(texts_dir) if f.endswith('.txt')]

all_errors = {}
for f in txt_files:
    path = os.path.join(texts_dir, f)
    errs = validate_scenario(path)
    if errs:
        all_errors[f] = errs

if not all_errors:
    print("No errors found.")
else:
    for f, errs in all_errors.items():
        print(f"--- {f} ---")
        for e in errs:
            print(e)
