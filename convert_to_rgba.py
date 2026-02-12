#!/usr/bin/env python3
from PIL import Image

def ensure_rgba_transparency(image_path):
    print(f"Processing: {image_path}")
    img = Image.open(image_path)
    
    # Convert to RGBA if needed
    if img.mode != 'RGBA':
        print(f"  Converting from {img.mode} to RGBA")
        img = img.convert("RGBA")
    
    # Get pixel data
    datas = img.getdata()
    new_data = []
    transparent_count = 0
    
    for item in datas:
        # If pixel is white or very light (likely background), make it transparent
        # Otherwise keep the pixel
        if item[0] > 240 and item[1] > 240 and item[2] > 240:
            new_data.append((255, 255, 255, item[3] if len(item) == 4 else 255))
        elif item[0] < 30 and item[1] < 30 and item[2] < 30:
            # Dark pixels become transparent
            new_data.append((0, 0, 0, 0))
            transparent_count += 1
        else:
            # Keep other pixels as is
            new_data.append(item if len(item) == 4 else (item[0], item[1], item[2], 255))
    
    img.putdata(new_data)
    img.save(image_path, "PNG")
    print(f"  Converted {transparent_count} pixels to transparent")
    print(f"  Final size: {img.size}")
    print()

if __name__ == "__main__":
    images = [
        "Assets/Game/HotResources/RawAssets/Texture/Decrease.png",
        "Assets/Game/HotResources/RawAssets/Texture/Edit.png"
    ]
    
    for img_path in images:
        ensure_rgba_transparency(img_path)
    
    print("All images processed successfully!")
