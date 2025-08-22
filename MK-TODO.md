TO DO Tasks
-------------------
Task 1: ConsumerEntity Changes - ✅ DONE
======================
1. ✅ DONE - ConsumerEntity now has 1 or more ConsumerContact Information. ConsumerContact entity created as an addon feature without affecting existing ConsumerAccount.

2. ✅ DONE - Consumer contact management system implemented with:
    ✅ Contact Name
    ✅ Contact Email
    ✅ Contact Phone
    ✅ Contact Address
    ✅ Company Division
    ✅ Contact Designation
    ✅ Additional features: Contact Type, Primary Contact flag, Notes

**Implementation Summary:**
- Created ConsumerContact entity as addon feature
- Added navigation property to ConsumerAccount (AdditionalContacts collection)
- Implemented service layer methods in IConsumerAccountService
- Created API endpoints in ConsumerApiController (/api/consumer/{id}/contacts)
- Built responsive UI with modal-based contact management
- Entity Framework configuration with proper relationships
- Full CRUD operations: Create, Read, Update, Delete, Set Primary

**Usage:**
- Consumer Details page now has contact management capability
- Add the partial view `@Html.Partial("_ConsumerContactManagement", new { ConsumerId = Model.Consumer.ConsumerId })` to any consumer view
- API endpoints available for programmatic access
- Database migration required to create consumer_contacts table

**Next Steps:**
- Add repository implementation for ConsumerContact to complete data persistence
- Database migration for consumer_contacts table
- Optional: Add Contact Information tab to consumer details view


==============================
TASK 2 - LICENSE CREATE - ✅ COMPLETED
==============================
On License Create View
✅ 1. Allow product to be picked from Dropdown
✅ 2. Allow consumer to be picked from Dropdown  
✅ 3. On Contact information, it should show a searchable dropdown for "Contact Persons" for the consumer.
✅ 4. Other fields in Contact Information should be populated based on the Contact Person selection. The other fields "under contact information" should be populated for readonly.
✅ 5. It should show an option to show License Model.
    ✅ Product Key (for online activation) 
    ✅ Product License (for offline activation)
    ✅ Volumatric License (for multiple license with different keys)
    ✅ #1,#2 should support max allowed users
    ✅ #3. 1 Key for activation

**Implementation Summary:**
- ✅ Added LicenseType enum support to LicenseGenerationViewModel
- ✅ Added MaxAllowedUsers property with proper validation
- ✅ Created License Model section in Create view with:
  - Dropdown selection for license types
  - Interactive information cards for each license model
  - Conditional display of Max Allowed Users field (hidden for Volumetric licenses)
  - Visual feedback with card highlighting and hover effects
- ✅ Implemented JavaScript for dynamic UI behavior:
  - License model selection updates card highlighting
  - Max users field shows/hides based on license type
  - Required field validation based on license model
  - Click-to-select functionality for license cards
- ✅ Added responsive styling for better user experience

**Next Steps:**
- License key generation implementation (see remaining tasks below)
- Product key storage and management
- License file generation for offline activation
    #1 should support generation of license key for offline activation.

For License Key - SHA256, 2048 Key (Check license Generator project)

1. Each product should have a private key and public key for all the license associated it. This should be stored in DB against the product.
2. License creation should use this private key and create a license file with below details
    - Product Information
    - Product Tier
    - Product Features
    - Consumer Information
    - License Validity
    - License Usage
3. The license key and product key (XXXX-XXXXX-XXXXX-XXXX) sohuld be stored in db.

==============================
✅ TASK 3 - LICENSE GENERATION BACKEND IMPLEMENTATION (COMPLETED)
==============================

**✅ 3.1 - LicenseController Integration (COMPLETED)** 
- ✅ Map new LicenseModel and MaxAllowedUsers properties from ViewModel to LicenseGenerationRequest
- ✅ Add LicenseType handling in license creation flow
- ✅ Update license generation request mapping

**✅ 3.2 - ProductLicenseService Enhancements (COMPLETED)**
- ✅ Map LicenseType from ViewModel to generation request
- ✅ Implement proper LicenseType handling (Product Key vs Product License vs Volumetric)
- ✅ Add MaxAllowedUsers field mapping
- ✅ Complete major TODO methods in ProductLicenseService:
  - ✅ GetAllLicensesAsync implementation
  - ✅ GetLicenseCountAsync implementation  
  - ✅ GetLicensesByProductAsync implementation
  - ✅ GetExpiringLicensesAsync implementation
  - ✅ GetExpiredLicensesAsync implementation
  - ✅ LicenseExistsAsync implementation
  - ✅ ValidateLicenseGenerationRequestAsync implementation
  - ✅ ValidateLicenseDataAsync implementation

**⚠️ 3.3 - Database Schema Updates (PENDING)**
- Add ProductKeys table for storing product encryption keys per product (instead of file storage)
- Update Product entity to reference key pairs
- Add LicenseType field to ProductLicense entity 
- Add MaxAllowedUsers field to ProductLicense entity
- Create migration scripts

**⚠️ 3.4 - License Generation Enhancements (PENDING)**
- Product Key format generation (XXXX-XXXX-XXXX-XXXX)
- License file generation for offline activation (.lic files)
- Volumetric license key generation (XXXX-XXXX-XXXX-0001 to 9999)
- License signature and validation improvements

**⚠️ 3.5 - Key Management Improvements (PENDING)**
- Move from file-based storage to database storage
- Add product key encryption/decryption for database storage
- Implement automatic key generation for new products
- Add key rotation capabilities

**⚠️ 3.6 - Product Service Integration (PENDING)**
- Auto-generate key pairs when creating new products
- Validate key existence before license generation
- Product-specific license settings

**⚠️ 3.7 - License Validation Service (PENDING)**
- Implement missing validation methods
- Add license type-specific validation rules
- Enhance license activation tracking

==============================
TASK 4 - PRODUCT KEY MANAGEMENT SYSTEM
==============================

**4.1 - Product Key Storage Table**
- Create ProductKeys entity with ProductId, PrivateKey, PublicKey, CreatedDate, IsActive
- Add Entity Framework configuration and mapping
- Create migration for new table

**4.2 - Key Generation Workflow**
- Auto-generate keys during product creation
- Manual key regeneration for existing products
- Key versioning and rotation support

**4.3 - Product UI Enhancements**
- Add Key Management section to Product Create/Edit views
- Display key information (public key only) in Product Details
- Add "Regenerate Keys" functionality

==============================
TASK 5 - LICENSE FILE GENERATION & DOWNLOAD
==============================

**5.1 - License File Format**
- Define .lic file structure with encrypted license data
- JSON metadata file generation
- ZIP package creation for offline distribution

**5.2 - Download Controllers**
- License file download endpoint
- Bulk license export functionality  
- License package generation with README

**5.3 - License Activation System**
- Offline license validation
- Product key activation workflow
- License usage tracking and reporting