# Letterhead Editor - Visual Diagrams & Reference

## 1. User Interface Layout

```
Company Profile Page
┌─────────────────────────────────────────────────────────────┐
│ Company Profile                                   [Success!] │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────┐  ┌──────────────────────────┐ │
│  │  Company Details         │  │  Letterhead              │ │
│  │  ────────────────────    │  │  (appears on all prints) │ │
│  │                          │  │  ┌────────────────────┐ │ │
│  │  Name:                   │  │  │ [+ Template]       │ │ │
│  │  [Mitshu            ___] │  │  ├────────────────────┤ │ │
│  │                          │  │  │                    │ │ │
│  │  Logo:                   │  │  │   QUILL EDITOR     │ │ │
│  │  [Choose File ____]      │  │  │   (Rich HTML)      │ │ │
│  │  [Current Logo  ▯]       │  │  │                    │ │ │
│  │                          │  │  │   Click here and   │ │ │
│  │  Address:                │  │  │   edit content     │ │ │
│  │  [Street/Area        __] │  │  │   visual editor    │ │ │
│  │                          │  │  │                    │ │ │
│  │  City:  [City___] City   │  │  │ Toolbar:           │ │ │
│  │  [State___]       [####] │  │  │ B I U Colors etc.  │ │ │
│  │                          │  │  │                    │ │ │
│  │  ... more fields ...     │  │  │  [Height: 440px]   │ │ │
│  │                          │  │  │                    │ │ │
│  └──────────────────────────┘  │  └────────────────────┘ │ │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ [Save]                                              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## 2. Template Button Functionality

```
User Interaction Flow
═════════════════════

Initial State:
┌──────────────────────────────────┐
│ Letterhead                        │
│ (appears on all print views)      │
│ ┌────────────────────────────────┐│
│ │  [+ Template]   [Save]         ││
│ ├────────────────────────────────┤│
│ │                                ││
│ │   Empty Editor                 ││
│ │   (Waiting for content)        ││
│ │                                ││
│ └────────────────────────────────┘│
└──────────────────────────────────┘

                ↓ [Click + Template]

After Template Insertion:
┌──────────────────────────────────┐
│ Letterhead                        │
│ (appears on all print views)      │
│ ┌────────────────────────────────┐│
│ │  [+ Template]   [Save]         ││
│ ├────────────────────────────────┤│
│ │                                ││
│ │  ॥ Shree Hinglaj... ॥         ││
│ │  ┌─────────┬─────────┬─────┐  ││
│ │  │Logo     │Company  │Info │  ││
│ │  │         │Mitshu   │Box  │  ││
│ │  │         │TEXTILE  │     │  ││
│ │  └─────────┴─────────┴─────┘  ││
│ │                                ││
│ │  ← Click to edit any part      ││
│ │                                ││
│ └────────────────────────────────┘│
└──────────────────────────────────┘

                ↓ [Edit & Save]

Saved to Database:
┌──────────────────────────────────┐
│ Success!                          │
│ Company profile saved successfully│
└──────────────────────────────────┘
```

## 3. Toolbar Options

```
Quill Toolbar Layout
════════════════════

┌─────────────────────────────────────────────────────┐
│ [Font ▼] [Size ▼] B I U ⊘ [Color▼][BG▼][Align▼]  │
│ [List▼] [OL▼] [Link] [Image] [Script▼] [Code] [X]  │
└─────────────────────────────────────────────────────┘

Legend:
├─ Font ▼      = Font selector (Arial, Helvetica, etc.)
├─ Size ▼      = Font size (Small, Normal, Large, Huge)
├─ B           = Bold
├─ I           = Italic
├─ U           = Underline
├─ ⊘           = Strikethrough
├─ Color▼      = Text color picker
├─ BG▼         = Background color picker
├─ Align▼      = Text alignment (Left, Center, Right)
├─ List▼       = Bullet list
├─ OL▼         = Ordered (numbered) list
├─ Link        = Insert/edit hyperlink
├─ Image       = Insert image
├─ Script▼     = Superscript/Subscript
├─ Code        = Code block formatting
└─ X           = Clear all formatting
```

## 4. Template Structure

```
Professional 3-Column Template
═══════════════════════════════

┌──────────────────────────────────────────────────┐
│  ॥ SHREE HINGLAJ MATAJI ॥ ... (Religious text) │ ← Header text
├────────────────────────────────────────────────┤
│                                                  │
│  ┌──────────┬────────────────┬───────────────┐ │
│  │          │                │               │ │
│  │  Logo    │  Company Name  │  Contact Box  │ │
│  │  (20%)   │    (30%)       │   (50%)       │ │
│  │          │  Mitshu        │ ☎ +91 94264   │ │
│  │ [Image]  │  TEXTILE       │ 📍 Address    │ │
│  │          │                │ ✉️ Email      │ │
│  │          │                │               │ │
│  └──────────┴────────────────┴───────────────┘ │
│                                                  │
└──────────────────────────────────────────────────┘
         ↓ (Document content starts here)
```

## 5. Editing Workflow

```
Content Editing Flowchart
════════════════════════

                Start
                  ↓
        Open Company Profile
                  ↓
          Look for Letterhead
                  ↓
         ┌─ Already has content? ─┐
         │                        │
        No                       Yes
         │                        │
         │                        ↓
         │                   Display in Editor
         │                        │
         ↓                        ↓
    Empty Editor          ┌─ Want to modify? ─┐
         │                │                  │
         ↓               No                 Yes
    + Template            │                  │
         │                ↓                  ↓
         ↓           Leave as is        Edit Content
    Template Inserted      │                 │
         │                 ↓                 ↓
         └────────→ Click Save ←────────────┘
                     │
                     ↓
              HTML synced to input
                     │
                     ↓
              Form submitted
                     │
                     ↓
              Save to database
                     │
                     ↓
           Success message shown
                     │
                     ↓
         Next print includes header
                     ↓
                   Done
```

## 6. Data Flow Diagram

```
Data Flow: From Editor to Print
══════════════════════════════

Admin Types     Quill        Hidden      Browser    Server
in Editor    Auto-syncs    Input Tag   Submits    Validates
   │             │            │          │          │
   │ "Mitshu" ──→│ HTML ──────→│ Form ──→│ POST ───→│
   │             │  String    │ Data    │ /Save    │
   │             │ (semantic) │         │          │ Saves
   │             │            │         │          │ to DB
   │             │            │         │          │
   └─────────────┴────────────┴─────────┴──────────→ CompanyProfile
                                                    Entity
                                                     │
                        ┌────────────────────────────┘
                        │
                        ↓
                  Next Print Job
                        │
            ┌───────────┴───────────┐
            ↓                       ↓
        Letterhead Partial    Document Content
            │                       │
            ├─ Retrieve HTML ─→ Combine ←─ Get Data
            │                       │
            └───────────┬───────────┘
                        │
                        ↓
                    Render PDF
                        │
                        ↓
            ┌──────────────────────┐
            │   FULL DOCUMENT      │
            │  ┌──────────────────┐│
            │  │  HEADER (HTML)   ││
            │  ├──────────────────┤│
            │  │  CONTENT (Data)  ││
            │  └──────────────────┘│
            └──────────────────────┘
```

## 7. Database Schema (Relevant Part)

```
CompanyProfile Table
════════════════════

┌─────────────────────────────────────┐
│       CompanyProfile                │
├─────────────────────────────────────┤
│ Id (int) ........................ PK │
│ Name (string)                       │
│ LogoFileName (string)               │
│ Address (string)                    │
│ City (string)                       │
│ State (string)                      │
│ Pincode (string)                    │
│ Phone (string)                      │
│ Mobile (string)                     │
│ Email (string)                      │
│ Website (string)                    │
│ GSTIN (string)                      │
│ PAN (string)                        │
│ CIN (string)                        │
│ ╔═════════════════════════════════╗ │
│ ║ LetterheadHtml (string)         ║ │ ← NEW FIELD
│ ║ (Stores HTML from editor)       ║ │
│ ║ Example:                         ║ │
│ ║ "<div>...<table>...</table>..." ║ │
│ ╚═════════════════════════════════╝ │
└─────────────────────────────────────┘

Existing since Migration:
  20260608044122_AddCompanyProfile

No changes needed - field already exists!
```

## 8. Security & Access Control

```
Access Control Flow
═══════════════════

                    User Request
                         │
                         ↓
                  [Authorize Attribute]
                         │
              ┌──────────┴──────────┐
              │                     │
           Admin?                No admin
              │                     │
             Yes                    ↓
              │                Redirect to
              ↓                Login/Denied
          Can Access
        CompanyController
              │
              ├─→ [HttpGet] Index
              │   ├─ Load CompanyProfile
              │   ├─ Map to ViewModel
              │   └─ Display View
              │
              └─→ [HttpPost] Save
                  ├─ Validate Model
                  ├─ Process Logo (if any)
                  ├─ Save LetterheadHtml
                  └─ Return Success

Result:
  ✓ Only Admins can edit letterhead
  ✓ Non-admins get access denied
  ✓ All changes are logged (if audit enabled)
  ✓ Data is validated before saving
```

## 9. Print Output Example

```
Print Output
════════════

┌──────────────────────────────────────────┐
│  ॥ SHREE HINGLAJ MATAJI ॥ ॥ SHREE...   │  ← Letterhead
│                                          │     (From LetterheadHtml)
│  ┌──────┐  ┌────────────┐  ┌─────────┐ │
│  │Logo  │  │Mitshu      │  │☎ Contact│ │
│  │      │  │TEXTILE     │  │📍 Address│ │
│  └──────┘  └────────────┘  └─────────┘ │
├──────────────────────────────────────────┤
│                                          │
│  Program Details                         │
│  ┌───┬───────┬──────┬──────┬───────────┐ │  ← Document Content
│  │No │Design │Color │Size  │Quantity   │ │     (From Database)
│  ├───┼───────┼──────┼──────┼───────────┤ │
│  │1  │DES001 │Red   │10x10 │100 Meters │ │
│  │2  │DES002 │Blue  │12x12 │150 Meters │ │
│  │3  │DES003 │Green │14x14 │200 Meters │ │
│  └───┴───────┴──────┴──────┴───────────┘ │
│                                          │
│                                          │
└──────────────────────────────────────────┘
```

## 10. Color Reference

```
Brand Colors (From Template)
════════════════════════════

Primary Brand Color:
┌─────────┐
│ #c0544b │  RGB(192, 84, 75)  Red/Brown
└─────────┘

Used for:
  ├─ Religious text headers
  ├─ Company name text
  ├─ Contact box border
  ├─ Icons color
  └─ Visual accents

Alternative Colors:
  ├─ #004b87 (Navy Blue)
  ├─ #d9534f (Bootstrap Danger)
  ├─ #333333 (Dark Gray)
  ├─ #666666 (Medium Gray)
  ├─ #999999 (Light Gray)
  └─ #FFFFFF (White)
```

## 11. Responsive Behavior

```
Different Screen Sizes
══════════════════════

Desktop (≥ 768px):
┌─────────────────────────────────────┐
│  Left (Company)  │  Right (Editor)  │
│  ┌─────────────┐ │ ┌──────────────┐ │
│  │ Details     │ │ │   Editor     │ │
│  │ Form        │ │ │   440px      │ │
│  │             │ │ │   tall       │ │
│  └─────────────┘ │ └──────────────┘ │
└─────────────────────────────────────┘

Tablet (< 768px):
┌────────────────────┐
│  Details Form      │
│  ┌────────────────┐│
│  │ Editor         ││
│  │ 440px tall     ││
│  └────────────────┘│
└────────────────────┘

Mobile (< 576px):
┌──────────────────┐
│  Stacked Layout  │
│  ┌──────────────┐│
│  │Form          ││
│  └──────────────┘│
│  ┌──────────────┐│
│  │Editor        ││
│  │350px tall    ││
│  └──────────────┘│
└──────────────────┘
```

## 12. Error Handling

```
Error Cases & Recovery
══════════════════════

Case 1: Invalid HTML Input
  └─→ Quill generates semantic HTML
      └─→ Safe and valid
      └─→ No errors occur

Case 2: User not Admin
  └─→ Authorization check fails
      └─→ Redirect to Access Denied
      └─→ No data exposed

Case 3: Large HTML Content
  └─→ Stored in database
      └─→ SQL Server handles large strings
      └─→ No truncation (normally)

Case 4: Image Upload Fails
  └─→ Use external URL instead
      └─→ Or use existing image URL

Case 5: Print Page Missing Partial
  └─→ Letterhead won't show
      └─→ Contact developer
      └─→ Add: <partial name="_LetterheadPartial" />
```

---

**Reference Completed**: Visual diagrams and architecture documentation
**Ready to Use**: Yes ✅
