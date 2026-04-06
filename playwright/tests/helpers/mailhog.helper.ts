import { expect, APIRequestContext } from '@playwright/test';

export interface MailhogEmailOptions {
  toEmail: string;
  submittedAfter?: Date;
  maxWaitSeconds?: number;
  expectedContent?: {
    subject?: string | RegExp;
    body?: string | RegExp;
    bodyContains?: string[];
  };
}

export interface MailhogEmail {
  Created: string;
  Subject: string;
  Body: string;
  BodyDecoded: string;
  To: string[];
  From: string;
  Raw: any;
}

/**
 * Helper function to verify an email was received in Mailhog
 * @param request - Playwright API request context
 * @param options - Email search and verification options
 * @returns The found email with decoded body
 */
export async function verifyEmailInMailhog(
  request: APIRequestContext,
  options: MailhogEmailOptions
): Promise<MailhogEmail> {
  const {
    toEmail,
    submittedAfter,
    maxWaitSeconds = 10,
    expectedContent
  } = options;

  // Wait for email to be delivered
  await new Promise(resolve => setTimeout(resolve, 2000));

  // Fetch emails from Mailhog API
  const response = await request.get('http://localhost:8025/api/v2/messages');
  expect(response.ok()).toBeTruthy();

  const mailhogData = await response.json();

  // Find email sent to the specified address
  const email = mailhogData.items?.find((msg: any) =>
    msg.Raw?.To?.some((to: any) => to.includes(toEmail))
  );

  expect(email, `Email to ${toEmail} not found in Mailhog`).toBeDefined();

  // Verify email timestamp if submittedAfter is provided
  if (submittedAfter) {
    const emailDate = new Date(email.Created);
    const timeDiff = (emailDate.getTime() - submittedAfter.getTime()) / 1000;

    expect(timeDiff, `Email was sent before submission (${timeDiff}s difference)`).toBeGreaterThan(-5); // Allow 5s clock skew
    expect(timeDiff, `Email took too long to arrive (${timeDiff}s)`).toBeLessThan(maxWaitSeconds);

    console.log(`✓ Email sent ${timeDiff.toFixed(1)}s after submission`);
  }

  // Decode base64 email body
  const emailBodyBase64 = email?.Content?.Body || '';
  const bodyDecoded = Buffer.from(emailBodyBase64, 'base64').toString('utf-8');

  // Verify expected content
  if (expectedContent) {
    if (expectedContent.subject) {
      const subject = email.Content?.Headers?.Subject?.[0] || '';
      if (typeof expectedContent.subject === 'string') {
        expect(subject).toContain(expectedContent.subject);
      } else {
        expect(subject).toMatch(expectedContent.subject);
      }
    }

    if (expectedContent.body) {
      if (typeof expectedContent.body === 'string') {
        expect(bodyDecoded).toContain(expectedContent.body);
      } else {
        expect(bodyDecoded).toMatch(expectedContent.body);
      }
    }

    if (expectedContent.bodyContains) {
      for (const text of expectedContent.bodyContains) {
        expect(bodyDecoded, `Email body should contain "${text}"`).toContain(text);
      }
    }
  }

  console.log(`✓ Confirmation email verified for ${toEmail}`);

  return {
    Created: email.Created,
    Subject: email.Content?.Headers?.Subject?.[0] || '',
    Body: emailBodyBase64,
    BodyDecoded: bodyDecoded,
    To: email.Raw?.To || [],
    From: email.Raw?.From || '',
    Raw: email
  };
}

/**
 * Clear all emails from Mailhog (useful for test isolation)
 */
export async function clearMailhog(request: APIRequestContext): Promise<void> {
  const response = await request.delete('http://localhost:8025/api/v1/messages');
  expect(response.ok()).toBeTruthy();
  console.log('✓ Mailhog cleared');
}
