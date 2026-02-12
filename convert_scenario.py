import re
import os

# Configuration
INPUT_FILE = r"C:\Users\hippi\Production exhibition\Production-exhibition-2025\Assets\Resources\TEMP\導入.txt"
OUTPUT_FILE = r"C:\Users\hippi\Production exhibition\Production-exhibition-2025\Assets\Resources\texts\scenario2.txt"

# Mappings
char_to_id = {
    "私": "robin",
    "スズメ": "robin",
    "カラス": "crow",
    "ミヤマガラス": "crow",
    "ツグミ": "thrush",
    "ハエ": "flies",
    "雄牛": "bull",
    "ハト": "pigeons",
    "甲虫": "beetle",
    "魚": "fish",
    "ミソサザイ": "wren",
    "フクロウ": "owl",
    "トビ": "tobi",
    "ヒワ": "goldfinch",
    "ヒバリ": "skylark"
}

id_to_sprite = {
    "robin": "robin",
    "crow": "crow_humams",
    "thrush": "thrush",
    "flies": "flies",
    "bull": "bull",
    "pigeons": "pigeons",
    "beetle": "beetle",
    "fish": "fish",
    "wren": "wren",
    "owl": "owl",
    "tobi": "tobi",
    "goldfinch": "goldfinch",
    "skylark": "skylark"
}

# State
current_speaker_id = None

def get_speaker_id(name_text):
    # Handle "？？？(Name)" format
    match = re.search(r"（(.+?)）", name_text.replace("(", "（").replace(")", "）"))
    if match:
        key = match.group(1)
    else:
        key = name_text
    
    return char_to_id.get(key, None)

def process_file():
    global current_speaker_id
    
    with open(INPUT_FILE, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    output_lines = []
    
    # Header
    output_lines.append("#start - 導入")
    output_lines.append("&!bg=sprite:black_bg")
    output_lines.append("&!bgm=new:theater")
    output_lines.append("&!ui=textpanel:1")
    
    # Initialize Characters (Hidden)
    for cid, sprite in id_to_sprite.items():
        output_lines.append(f"&!ncr=name:{cid},sprite:{sprite}")
        output_lines.append(f"&!cr=name:{cid},show:0")
        
    for line in lines:
        line = line.strip()
        if not line:
            output_lines.append("")
            continue
            
        # Handle Page/Text lines
        if line.startswith("&"):
            # Check for speech
            # Expected format: &Name「Text」 or &「Text」 or &Name(Suffix)「Text」
            # Split by key bracket
            parts = line.split("「", 1)
            
            if len(parts) == 2:
                # Speech line
                name_part = parts[0][1:].strip() # Remove & and whitespace
                content = "「" + parts[1]
                
                # Identify Speaker
                new_speaker_id = get_speaker_id(name_part)
                
                # Character switching logic
                cmds = ""
                if new_speaker_id:
                    if new_speaker_id != current_speaker_id:
                        if current_speaker_id:
                            cmds += f"!cr=name:{current_speaker_id},show:0"
                        cmds += f"!cr=name:{new_speaker_id},show:1"
                        current_speaker_id = new_speaker_id
                    
                    # Prepend commands to the line or previous line?
                    # Format standard: &!cmd... \n &Text
                    if cmds:
                        output_lines.append(f"&{cmds}")
                
                # Reconstruct line
                output_lines.append(line)
                
            else:
                # Probably narration or monologue without brackets (unlikely based on file)
                # Or just standard narration
                output_lines.append(line)
                
        # Handle Choice Placeholder
        elif line.startswith("選択肢"):
            # Hardcode the specific choice found in text
            output_lines.append("&!ch=冷静になって反論する^branch_1")
            
        # Handle Labels
        elif line.startswith("#"):
            # Check if it matches the choice destination
            if "冷静になって" in line:
                output_lines.append("#branch_1 - 冷静になって反論する")
            else:
                output_lines.append(line)
        
        # Handle Note/Other (Introduction, etc)
        elif line == "導入":
            continue # Already handled in header
            
        else:
            # Just keep comments/newlines
            output_lines.append(line)

    # Write output
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write('\n'.join(output_lines))
        
    print(f"Converted {len(lines)} lines to {len(output_lines)} lines.")

if __name__ == "__main__":
    process_file()
