import os
from PIL import Image
image = Image.open('./test.png')
image = image.convert('RGB')
output_path = os.path.join('./test.jpg')
# image.save(output_path, "JPEG", quality=1)
image.save(output_path, quality=1)
# im10 = Image.open(IMAGE_10)