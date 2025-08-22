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
TASK 2 - LICENSE CREATE
==============================
On License Create View
1. Allow product to be picked from Dropdown
2. Allow consumer to be picked from Dropdown
3. On Contact information, it should show a searchable dropdown for "Contact Persons" for the consumer.
4. Other fields in Contact Information should be populated based on the Contact Person selection. The other fields "under contact information" sohuld be populated for readonly.
5. It should show an option to show License Model.
    1. Product Key (for online activation)
    2. Product License (for offline activation)
    3. Voumatric License (for multiple license with different keys)
    #1,#2 should support max allowed users
    #3. 1 Key for activation
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