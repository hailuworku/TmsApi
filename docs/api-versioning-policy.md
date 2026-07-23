# TMS API Versioning Policy

This document defines how we manage changes and versions for the Training Management System (TMS) API.

## 1. Breaking Changes
A "Breaking Change" requires a new major version (e.g., moving from V1 to V2). Examples include:
*   Removing or renaming a field in a JSON response.
*   Changing an HTTP status code (e.g., changing 200 OK to 201 Created).
*   Adding new required validation rules that didn't exist before.

## 2. Additive Changes (Non-breaking)
These changes can be added to an existing version without breaking current clients:
*   Adding a new optional field to a response.
*   Creating a brand new API endpoint.
*   Adding a new optional query parameter.

## 3. Sunset Window
When a new major version is released, the previous version will remain active for at least **6 months**. This gives our partners enough time to update their applications.

## 4. Communication
We inform our users about version changes through:
*   **HTTP Headers:** We use `Deprecation` and `Sunset` headers on old versions.
*   **Documentation:** Updates are posted in the project CHANGELOG.
*   **Email:** Direct notification to all registered API consumers.

## 5. Skipping Versions
Clients are not forced to upgrade to every single version. For example, a client can move directly from V1 to V3 if they choose.