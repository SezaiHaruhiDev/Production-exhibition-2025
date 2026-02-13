
import re

file_path = '/Users/haruhisezai/Production-exhibition-2025/Assets/Resources/texts/scenario.txt'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
in_flashback = False

for i, line in enumerate(lines):
    # Detect start of flashback setup
    if '!ncr=name:sparrow' in line:
        in_flashback = True
    
    # Detect end of flashback (return to main character)
    if '!cr=name:main,show:1' in line or '主「' in line:
        in_flashback = False

    if in_flashback:
        # Check for dialogue focus
        if i + 1 < len(lines):
            next_line = lines[i+1]
            if '&駒鳥「' in next_line:
                if '!fc=name:robin' in line:
                    line = line.replace('!fc=name:robin', '!fc=name:sparrow')
            elif '&スズメ「' in next_line:
                if '!fc=name:sparrow' in line:
                    line = line.replace('!fc=name:sparrow', '!fc=name:robin')
        
        # Also check same line focus
        if '&!fc=' in line and '「' in line:
              if '駒鳥「' in line:
                  line = line.replace('!fc=name:robin', '!fc=name:sparrow')
              elif 'スズメ「' in line:
                  line = line.replace('!fc=name:sparrow', '!fc=name:robin')
    
    new_lines.append(line)

with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Flashback focus swapped where needed.")
