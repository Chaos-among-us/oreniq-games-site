# Launch Setup Click By Click

This is the simplest path to finish the legal and QA hosting setup without buying a full website.

## Part 1: Create A Support Email
1. Create a support inbox you control, for example:
   - `support@yourdomain.com`
   - or a dedicated Gmail like `oreniqgames.support@gmail.com`
2. Keep this address for:
   - Google Play support contact
   - privacy contact
   - refund questions
   - tester/legal contact

## Part 2: Publish The Privacy Policy And Terms With GitHub Pages
You do not need a separate website for this. GitHub Pages is enough.

### Files already prepared in this repo
- `docs/index.html`
- `docs/privacy-policy.html`
- `docs/terms-of-service.html`
- `docs/legal-site.css`

### Edit the placeholders
1. Open:
   - `docs/privacy-policy.html`
   - `docs/terms-of-service.html`
2. Replace every placeholder in brackets, especially:
   - `[effective-date]`
   - `[legal-entity-name]`
   - `Cavern Veerfall`
   - `[support-email]`
   - `[privacy-contact-email]`
   - `[mailing-address-or-business-address]`
   - `[governing-law]`
   - `[dispute-venue]`

### Turn on GitHub Pages
1. Open the GitHub repo in a browser.
2. Click `Settings`.
3. In the left sidebar, click `Pages`.
4. Under `Build and deployment`, set `Source` to `Deploy from a branch`.
5. Under `Branch`, choose:
   - `master`
   - folder `/docs`
6. Click `Save`.
7. Wait for GitHub Pages to finish publishing.

### Your final public URLs
- Privacy policy:
  - `https://chaos-among-us.github.io/Block-dodger1/privacy-policy.html`
- Terms:
  - `https://chaos-among-us.github.io/Block-dodger1/terms-of-service.html`

## Part 3: Update The In-App Legal Links
1. Open:
   - `Assets/Resources/AppLegalConfig.json`
2. Confirm the URLs match your live GitHub Pages URLs.
3. Add your support email in `supportEmail`.
4. Rebuild the app.
5. Launch the app and confirm the new `Privacy` button on the main menu opens the live page.

## Part 4: Replace The Laptop LAN QA Upload With The Hosted Worker
1. Follow:
   - `docs/QA_REMOTE_GITHUB_UPLOAD_SETUP.md`
2. When you get the Worker URL, open:
   - `Assets/Resources/QaSubmissionConfig.json`
3. Replace `uploadUrl` with:
   - `https://YOUR-WORKER-URL/qa-upload`
4. Rebuild the QA APK.
5. Install it on the phone.
6. After that, testers can upload from anywhere with internet access, and this laptop does not need to stay on.

## Part 5: Play Console Entries To Fill
1. Open Play Console.
2. Go to `App content`.
3. Fill:
   - Privacy policy URL
   - Ads declaration
   - Target audience and content
   - App access
   - Data safety
   - Content rating
4. Use these repo drafts as your source:
   - `docs/legal/LEGAL_LAUNCH_PACK.md`
   - `docs/legal/PRIVACY_POLICY_DRAFT.md`
   - `docs/legal/TERMS_OF_SERVICE_DRAFT.md`
   - `docs/legal/GOOGLE_PLAY_DATA_SAFETY_WORKSHEET.md`

## Part 6: Release Decision Before You File Data Safety
1. Decide whether the shipping build keeps:
   - QA screen recording
   - tester names
   - tester notes
   - QA uploads
2. If `no`, remove those systems before the production build and answer Data safety from the cleaned build.
3. If `yes`, keep the disclosure flow and include those flows in the privacy policy and Data safety answers.
