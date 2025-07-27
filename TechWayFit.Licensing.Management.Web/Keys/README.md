# TechWayFit License Generator - RSA Key Storage

This directory contains the RSA key pair used for license generation and validation.

## Files

- `private_key.pem` - RSA private key (2048-bit) used for signing licenses
- `public_key.pem` - RSA public key used for license validation

## Security Notes

- The private key file (`private_key.pem`) has restricted permissions (600) - readable only by owner
- **NEVER commit the private key to version control**
- The public key can be safely shared and deployed with the API Gateway
- Keys are automatically generated on first run if not present
- Keys persist across application restarts and builds

## Key Management

Use the License Generator WebUI to:
- Generate new key pairs
- Export public keys for deployment
- Save/backup private keys with password protection
- Load private keys from backup files

## Deployment

For production deployment:
1. Copy the `public_key.pem` content to the API Gateway's `appsettings.json`
2. Backup the `private_key.pem` securely
3. Ensure this directory has proper file permissions in production
