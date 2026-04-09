import fitz

pdf_path = "Documents/7팀-설계서v1.0-AI트랙-허서준-송힘찬-이지관-최현빈-홍준표.pdf"
doc = fitz.open(pdf_path)

target_pages = [36, 109, 122, 123, 126, 129, 131]
for i in target_pages:
    page = doc.load_page(i)
    for idx, img in enumerate(page.get_images(full=True)):
        xref = img[0]
        base = doc.extract_image(xref)
        with open(f"ui_page_{i}_{idx}.{base['ext']}", "wb") as f:
            f.write(base["image"])
print("Done")
