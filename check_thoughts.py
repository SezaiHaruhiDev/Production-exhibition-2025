
import re

file_path = '/Users/haruhisezai/Production-exhibition-2025/Assets/Resources/texts/scenario.txt'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

current_focus = None
results = []

for i, line in enumerate(lines):
    if '!fc=' in line:
        m = re.search(r'!fc=name:([^,! \n&!#]+)', line)
        if m:
            current_focus = m.group(1).strip()
        elif '!fc=reset' in line:
            current_focus = 'reset'
    
    # Check for thought sentence &「...」
    if line.strip().startswith('&「'):
        if current_focus != 'robin':
            results.append((i+1, current_focus, line.strip()))

if not results:
    print("All thought sentences seem to be focused on robin.")
else:
    for line_num, focus, content in results:
        print(f"Line {line_num}: Focus is '{focus}', but thought sentence starts: {content}")
