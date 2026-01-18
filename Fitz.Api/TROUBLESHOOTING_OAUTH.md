# Discord OAuth "invalid_grant" Error Troubleshooting

## Overview
The "invalid_grant" error occurs during the OAuth token exchange process when Discord rejects the authorization code. This document outlines common causes and solutions.

## Common Causes

### 1. Redirect URI Mismatch
**Most Common Cause**: The redirect URI used in the token exchange must match EXACTLY:
- The redirect URI used in the initial authorization request
- The redirect URI configured in your Discord OAuth application settings

**Symptoms:**
- Error: `invalid_grant`
- Error description may mention redirect URI mismatch

**Solution:**
1. Check your Discord OAuth app settings at https://discord.com/developers/applications
2. Verify the redirect URI in your Discord app matches exactly (including protocol, port, and path)
3. Ensure the frontend uses the same redirect URI in both:
   - Login page (`login/page.tsx`) - authorization request
   - Callback page (`callback/page.tsx`) - token exchange request
4. Ensure the backend environment variable `DISCORD_REDIRECT_URI` matches (if used for validation)

**Example:**
- Discord App Setting: `http://localhost:5173/callback`
- Frontend Login: `http://localhost:5173/callback` ✓
- Frontend Callback: `http://localhost:5173/callback` ✓
- Backend Config: `http://localhost:5173/callback` ✓

### 2. Authorization Code Already Used
**Cause**: Authorization codes are single-use. If the code is exchanged twice, the second attempt will fail.

**Symptoms:**
- Error: `invalid_grant`
- Error description: "Invalid authorization code"

**Solution:**
- Ensure the callback page only calls the exchange endpoint once
- Check for duplicate requests (network tab, retry logic)
- Verify the authorization code isn't being cached and reused

### 3. Authorization Code Expired
**Cause**: Authorization codes expire after 10 minutes.

**Symptoms:**
- Error: `invalid_grant`
- Error description may mention expiration

**Solution:**
- Ensure users complete the OAuth flow promptly
- If the user takes too long, they'll need to restart the login process

### 4. Client ID/Secret Mismatch
**Cause**: The client ID or secret doesn't match your Discord OAuth application.

**Symptoms:**
- Error: `invalid_grant` or `invalid_client`

**Solution:**
1. Verify `DISCORD_CLIENT_ID` and `DISCORD_CLIENT_SECRET` environment variables
2. Check that they match your Discord OAuth app credentials
3. Ensure no extra whitespace or encoding issues

### 5. Redirect URI Not Registered in Discord
**Cause**: The redirect URI hasn't been added to your Discord OAuth application's allowed redirect URIs.

**Symptoms:**
- Error: `invalid_grant`
- May occur during authorization redirect

**Solution:**
1. Go to https://discord.com/developers/applications
2. Select your application
3. Navigate to OAuth2 → General
4. Add your redirect URI to the "Redirects" section
5. Save changes

## Debugging Steps

### 1. Check Server Logs
The enhanced logging will show:
- Redirect URI being used
- Client ID prefix (first 10 characters)
- Full error response from Discord
- Status code

Look for log entries like:
```
Discord OAuth token exchange failed. StatusCode: 400, Error: invalid_grant, ErrorDescription: ..., RedirectUri: ...
```

### 2. Verify Environment Variables
**Backend:**
- `DISCORD_CLIENT_ID`
- `DISCORD_CLIENT_SECRET`
- `DISCORD_REDIRECT_URI` (optional, used for validation)

**Frontend:**
- `NEXT_PUBLIC_DISCORD_CLIENT_ID`
- `NEXT_PUBLIC_DISCORD_REDIRECT_URI`

### 3. Compare Redirect URIs
Check that these match exactly:
1. Discord OAuth app redirect URI setting
2. Frontend login page redirect URI
3. Frontend callback page redirect URI
4. Backend `DISCORD_REDIRECT_URI` (if configured)

### 4. Test the Flow
1. Clear browser cookies/localStorage
2. Start fresh login flow
3. Monitor network requests:
   - Authorization request should redirect to Discord
   - Callback should receive `code` parameter
   - Token exchange should use the same redirect URI

## Quick Checklist

- [ ] Redirect URI matches in Discord app settings
- [ ] Redirect URI matches in frontend login page
- [ ] Redirect URI matches in frontend callback page
- [ ] Client ID and Secret are correct
- [ ] Authorization code is only used once
- [ ] Authorization code hasn't expired (within 10 minutes)
- [ ] No trailing slashes or protocol mismatches (http vs https)
- [ ] Port numbers match (if using localhost)

## Additional Notes

- Redirect URIs are case-sensitive for the path portion
- Protocol (http/https) must match exactly
- Port numbers must match exactly
- Trailing slashes matter: `/callback` ≠ `/callback/`
- Query parameters in redirect URI are not allowed by Discord
