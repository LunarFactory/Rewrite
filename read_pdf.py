import fitz
import sys

doc = fitz.open("Documents/7팀-설계서v1.0-AI트랙-허서준-송힘찬-이지관-최현빈-홍준표.pdf")
text = ""
for page in doc:
    text += page.get_text() + "\n"

with open("temp_pdf_text.txt", "w", encoding="utf-8") as f:
    f.write(text)
print("Extracted successfully.")
