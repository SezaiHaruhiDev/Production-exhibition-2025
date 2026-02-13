
import re

file_path = '/Users/haruhisezai/Production-exhibition-2025/Assets/Resources/texts/scenario.txt'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

current_focus = None
results = []

for i, line in enumerate(lines):
    # Detect focus command
    # &!fc=name:robin
    if '!fc=' in line:
        m = re.search(r'!fc=name:([^,! ]+)', line)
        if m:
            current_focus = m.group(1)
        elif '!fc=reset' in line:
            current_focus = 'reset'
        elif '!fc=offset' in line:
            pass # ignore offset
    
    # Detect thought bubble start
    if '!tb=state:true' in line:
        if current_focus != 'robin':
            results.append((i + 1, current_focus, line.strip()))

# Print findings
if not results:
    print("All thought blocks seem to have robin focus or no conflicting focus detected.")
else:
    for line_num, focus, content in results:
        print(f"Line {line_num}: Focus is '{focus}', but !tb=state:true starts: {content}")
