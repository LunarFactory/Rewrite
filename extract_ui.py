import fitz
import os

pdf_path = "Documents/7팀-설계서v1.0-AI트랙-허서준-송힘찬-이지관-최현빈-홍준표.pdf"
doc = fitz.open(pdf_path)

# Let's target pages with "화면 설계" specifically between page 100 to 150
for i in range(100, min(160, len(doc))):
    page = doc.load_page(i)
    text = page.get_text()
    if "Title" in text or "초기 화면" in text or "타이틀" in text or "로그인" in text or "관리자" in text:
        image_list = page.get_images(full=True)
        for img_index, img in enumerate(image_list):
            xref = img[0]
            base_image = doc.extract_image(xref)
            image_bytes = base_image["image"]
            image_ext = base_image["ext"]
            filename = f"extracted_ui_page{i}_{img_index}.{image_ext}"
            with open(filename, "wb") as f:
                f.write(image_bytes)
            print(f"Saved {filename}")

print("Done")
