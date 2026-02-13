
import re

file_path = '/Users/haruhisezai/Production-exhibition-2025/Assets/Resources/texts/scenario.txt'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
current_focus = None
fixes = 0

for i, line in enumerate(lines):
    # Update current focus
    if '!fc=' in line:
        m = re.search(r'!fc=name:([^,! \n&!#]+)', line)
        if m:
            current_focus = m.group(1).strip()
        elif '!fc=reset' in line:
            current_focus = 'reset'
    
    # Check if thought bubble starts
    if '!tb=state:true' in line:
        if current_focus != 'robin':
            # Inject robin focus before or with the tb command
            # If the line already has other commands, we should be careful.
            # Usually it's a line like &!tb=state:true
            # We can change it to &!fc=name:robin!tb=state:true
            if line.strip().startswith('&!'):
                new_line = line.replace('&!', '&!fc=name:robin!', 1)
                new_lines.append(new_line)
                current_focus = 'robin'
                fixes += 1
                continue
            else:
                # Unusual format, just prepend
                new_lines.append(f"&!fc=name:robin\n")
                new_lines.append(line)
                current_focus = 'robin'
                fixes += 1
                continue
    
    new_lines.append(line)

if fixes > 0:
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
    print(f"Applied {fixes} focus fixes to scenario.txt.")
else:
    print("No fixes needed.")
