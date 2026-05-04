import { Page } from '@playwright/test';

// Minimal ISOBMFF container with 'heic' brand.
// Browsers cannot generate or decode HEIC natively, so this stays synthetic.
// It is enough for our IsHeicFile() check (content-type + extension).
export const MINIMAL_HEIC = Buffer.from([
  0x00, 0x00, 0x00, 0x18, // box size: 24
  0x66, 0x74, 0x79, 0x70, // 'ftyp'
  0x68, 0x65, 0x69, 0x63, // major brand: 'heic'
  0x00, 0x00, 0x00, 0x00, // minor version
  0x68, 0x65, 0x69, 0x63, // compatible: 'heic'
  0x6D, 0x69, 0x66, 0x31, // compatible: 'mif1'
]);

// Generate real JPEG and PNG images by drawing on the browser's own canvas.
// These are guaranteed decodable by Chrome and will complete RequestImageFileAsync
// without hitting the 5s fallback timeout.
export async function generateRealImageFiles(page: Page) {
  const [jpegBase64, pngBase64] = await page.evaluate(() => {
    const canvas = document.createElement('canvas');
    canvas.width = 100;
    canvas.height = 100;
    const ctx = canvas.getContext('2d')!;

    // Green background (garden), brown square (chicken house)
    ctx.fillStyle = '#4CAF50';
    ctx.fillRect(0, 0, 100, 100);
    ctx.fillStyle = '#795548';
    ctx.fillRect(20, 40, 60, 60);

    return [
      canvas.toDataURL('image/jpeg', 0.8).split(',')[1],
      canvas.toDataURL('image/png').split(',')[1],
    ];
  });

  return [
    { name: 'garden-photo.jpg',  mimeType: 'image/jpeg', buffer: Buffer.from(jpegBase64, 'base64') },
    { name: 'chicken-house.png', mimeType: 'image/png',  buffer: Buffer.from(pngBase64, 'base64') },
    { name: 'iphone-photo.heic', mimeType: 'image/heic', buffer: MINIMAL_HEIC },
  ];
}
