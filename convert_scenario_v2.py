
import re

input_path = r"c:\Users\hippi\Production exhibition\Production-exhibition-2025\Assets\Resources\texts\scenario.txt"
output_path = r"c:\Users\hippi\Production exhibition\Production-exhibition-2025\Assets\Resources\texts\scenario_formatted.txt"

name_map = {
    '私': 'robin',
    '魚': 'fish',
    'カラス': 'crow',
    'ハト': 'pigeons',
    '雄牛': 'bull',
    'ハエ': 'flies',
    'ヒバリ': 'skylark',
    'フクロウ': 'owl',
    'ツグミ': 'thrush',
    '甲虫': 'beetle',
    'ミソサザイ': 'wren',
    'ヒワ': 'goldfinch',
    'トビ': 'tobi'
}

header = """#start - 転
&!bg=sprite:stage_6
&!out=sub:false
&!ui=textpanel:1

&!ncr=name:robin,sprite:robin
&!ncr=name:crow,sprite:crow_strangeform
&!ncr=name:thrush,sprite:thrush
&!ncr=name:flies,sprite:flies
&!ncr=name:bull,sprite:bull
&!ncr=name:pigeons,sprite:pigeons
&!ncr=name:beetle,sprite:beetle
&!ncr=name:fish,sprite:fish
&!ncr=name:wren,sprite:wren
&!ncr=name:owl,sprite:owl
&!ncr=name:tobi,sprite:tobi
&!ncr=name:goldfinch,sprite:goldfinch
&!ncr=name:skylark,sprite:skylark

&!cr=name:robin,show:1,pos:-2800^-600,size:1800^1800
&!cr=name:crow,show:1,pos:-2200^-600,size:1800^1800
&!cr=name:thrush,show:1,pos:-1600^-600,size:1300^1300
&!cr=name:fish,show:1,pos:-1000^-600,size:1800^1800
&!cr=name:flies,show:1,pos:-400^-600,size:1800^1800
&!cr=name:bull,show:1,pos:200^-600,size:1800^1800
&!cr=name:pigeons,show:1,pos:800^-600,size:1800^1800
&!cr=name:beetle,show:1,pos:1400^-600,size:1800^1800
&!cr=name:wren,show:1,pos:2000^-600,size:1800^1800
&!cr=name:owl,show:1,pos:2600^-600,size:1800^1800
&!cr=name:tobi,show:1,pos:3200^-600,size:1800^1800
&!cr=name:goldfinch,show:1,pos:3800^-600,size:1800^1800
&!cr=name:skylark,show:1,pos:4400^-600,size:1800^1800

"""

def process_file():
    with open(input_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    processed_lines = []
    
    # Add header
    processed_lines.append(header)

    for line in lines:
        line = line.strip()
        if not line:
            processed_lines.append("\n")
            continue
            
        if line.startswith("#"):
            processed_lines.append(line + "\n")
            continue
            
        if line.startswith("("):
             # Keep comments/instructions as plain text lines for now, but mark them
            processed_lines.append(f"TODO_INSTRUCTION: {line}\n")
            continue
            
        # Dialogue detection
        if "「" in line:
            parts = line.split("「", 1)
            name = parts[0]
            
            # Use name map if available
            char_id = name_map.get(name)
            
            if char_id:
                processed_lines.append(f"&!fc=name:{char_id}\n")
            
            # Format: &Name「Content
            processed_lines.append(f"&{name}「{parts[1]}\n")
            
        else:
            # Narration
            processed_lines.append(f"&!tb=state:true\n")
            processed_lines.append(f"&{line}\n")
            processed_lines.append(f"&!tb=state:false\n")

    with open(output_path, 'w', encoding='utf-8') as f:
        f.writelines(processed_lines)
        
    print(f"Processed {len(lines)} lines.")

if __name__ == "__main__":
    process_file()
