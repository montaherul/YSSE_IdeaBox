# YSSE Idea Box — Software Requirements Specification

**Version 3.2 (refined baseline)** · Supersedes v3.0 and v3.1 · Status: Ready for design and development planning

**Product concept:** From Ideas to Entrepreneurship

## 0. Document Control and Conventions

### 0.1 Purpose

This document is the authoritative baseline for designing the database, defining API contracts, creating UI screens, and generating implementation tasks for YSSE Idea Box.

### 0.2 Changes from v3.0

- Removed pasted rendering artifacts and rebuilt the idea status model as an explicit transition table.
- Resolved terminology collisions ("review" was used for both community feedback and staff screening). See Glossary.
- Added requirement priorities, measurable non-functional targets, data-retention defaults, and a ranking formula template.
- Clarified guest-review rules (first review, second-idea attempt, edits, moderation) as testable behaviors.
- Added a risks and open-decisions register (Section 26) so unresolved policy choices are visible rather than implied.
- Added traceability: each module ends with acceptance criteria that map to Section 24.

**Added in v3.2:** a Creator Profile (Section 6.1); open-access and inclusion requirements so anyone can use the Idea Box (Section 6.2); special granted roles and permissions (Section 4.4); problem posts, so people can raise a real problem even without a solution (Section 7.4); matching data entities (Section 20) and acceptance criteria (Section 24).

### 0.3 Requirement conventions

- **Requirement IDs** use the pattern `FR-<Module><NN>`. Non-functional requirements use `NFR-<NN>`.
- **Keywords:** *shall* = mandatory; *should* = recommended; *may* = optional.
- **Priority:** **M** = Must (required for first production release), **S** = Should (target for first release, may slip), **C** = Could (later release).
- Items marked **\[Config\]** are policy values that administrators can change without a code deployment.
- Items marked **\[Decision\]** need a business decision recorded in Section 26 before the related phase starts.

## 1. Executive Summary

YSSE Idea Box is a web-based platform for idea discovery and entrepreneurship facilitation. Members submit ideas; the community reviews them and votes in an official ranking process; entrepreneurs, investors, and mentors discover promising ideas and contact creators privately. A separate community marketplace lets members sell, donate, or exchange reusable items.

| Participant | What they do on the platform |
| --- | --- |
| Idea creators | Submit ideas, receive feedback, improve proposals, find collaborators |
| YSSE (intermediary) | Moderates, screens, ranks, and facilitates introductions |
| Entrepreneurs, investors, mentors | Discover ideas, send inquiries, and independently negotiate collaboration |
| Community | Reviews ideas (open feedback) and votes (official ranking, members only) |

### 1.1 Non-negotiable business boundary

**YSSE facilitates connections; it does not manage funding.**

- External entrepreneurs, investors, and sponsors evaluate opportunities independently.
- Negotiations, agreements, and money transfers happen directly between the parties.
- YSSE does not approve, hold, or distribute investment funds through this platform.
- The platform does not guarantee investment, business success, or commercial viability.
- Popularity (votes or reviews) never makes an idea automatically eligible for investment.
- The platform shall display this boundary wherever inquiries, outcomes, or matching are shown.

## 2. Objectives, Scope, and Success Measures

### 2.1 Objectives

1. Provide a structured environment for collecting and developing ideas.
2. Encourage public participation through reviews and constructive feedback.
3. Protect the integrity of official voting and rankings.
4. Connect promising ideas with relevant entrepreneurs and opportunities.
5. Protect creators who choose anonymous public attribution.
6. Enable private creator–entrepreneur communication.
7. Recognize valuable ideas and contributions.
8. Enable members to sell, donate, or exchange reusable items.
9. Help YSSE measure its effectiveness in facilitating connections.
10. Keep operations secure, auditable, and privacy-conscious.

### 2.2 In scope (first production release)

Public discovery, authentication and profiles, idea lifecycle and moderation, community reviews (guest and registered), voting and leaderboards, entrepreneur portal, private messaging, outcome reporting, marketplace, notifications, administration, and project collaboration tracking. Delivery is phased (Section 23).

### 2.3 Out of scope

See Section 25 (Explicit Exclusions).

### 2.4 Success measures (reported on the admin dashboard)

| Measure | Definition |
| --- | --- |
| Idea throughput | Ideas submitted → published, per month |
| Participation | Active reviewers and voters per voting period |
| Connection rate | Published ideas receiving at least one inquiry ÷ total published ideas |
| Response rate | Inquiries answered or declined within **\[Config\]** 14 days ÷ total inquiries |
| Reported outcomes | Count of self-reported and verified outcomes by type |

Targets for each measure are set by YSSE before launch **\[Decision\]**.

## 3. Glossary

| Term | Meaning |
| --- | --- |
| **Community review** | Open written feedback on a published idea, from a guest or a registered member. Previously called just "review". Not a vote. |
| **Screening review** | Staff evaluation of a submitted idea (originality, feasibility, relevance, impact) that leads to a publication decision. Performed by the *Screening Reviewer* role (formerly "Idea Reviewer"). |
| **Vote** | An official ranking signal cast only by an authenticated, eligible member. |
| **Guest** | A visitor without an account. |
| **Guest visitor ID** | A random, opaque server-issued identifier used for best-effort guest-review limits. It is not an identity. |
| **Creator** | The member who submitted an idea. Always recorded internally, even if shown publicly as anonymous. |
| **Opportunity seeker** | An authenticated entrepreneur, investor/sponsor, or mentor who may send inquiries. |
| **Inquiry** | A structured, private expression of interest sent to a creator about a published idea. |
| **Outcome** | A reported result of a connection (e.g., partnership formed). Self-reported unless independently verified. |
| **Published** | Visible in public discovery. Distinct from *shortlisted* and *approved for development*. |

## 4. User Roles and Permissions

The system shall use server-enforced role-based access control (RBAC) with resource-level checks (for example, "only the creator can edit this idea"). A user may hold multiple roles where authorized.

### 4.1 Roles

| Role | Summary |
| --- | --- |
| Public Visitor / Guest | Browse public content; submit one best-effort anonymous community review |
| Registered Member | Submit ideas, review, comment, follow, vote when eligible, use marketplace |
| Idea Creator | A member's relationship to their own ideas: manage ideas, answer inquiries, control identity disclosure |
| Entrepreneur | Business profile, watchlist, inquiries, private messaging |
| Investor / Sponsor | Same as entrepreneur, with support-interest declarations |
| Mentor / Expert | Expertise profile, discover relevant ideas, offer guidance through authorized channels |
| Moderator | Moderate comments, reviews, reports, and marketplace listings |
| Screening Reviewer | Evaluate submitted ideas and record publication decisions |
| Platform Administrator | Accounts, roles, categories, settings, voting periods, audit policy |

### 4.2 Permission matrix

✔ = allowed · ◐ = conditional · ✖ = not allowed

| Capability | Guest | Member | Opportunity seeker | Moderator | Screening Reviewer | Admin |
| --- | --- | --- | --- | --- | --- | --- |
| Browse published ideas | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Submit community review | ◐ one idea, best-effort | ✔ | ◐ (if also member) | ✔ | ✔ | ✔ |
| Cast official vote | ✖ | ◐ eligible | ◐ only if also eligible member | ◐ policy | ◐ policy | ◐ policy |
| Submit ideas | ✖ | ✔ | ◐ (if also member) | ✔ | ✔ | ✔ |
| Send inquiry to creator | ✖ | ✖ | ✔ | ✖ | ✖ | ✖ |
| Read private conversations | ✖ | participants only | participants only | ✖ | ✖ | ✖ exceptional, audited |
| Hide/remove reviews and comments | ✖ | own only | own only | ✔ | ✖ | ✔ |
| Publish / reject ideas | ✖ | ✖ | ✖ | ◐ policy | ✔ | ✔ |
| Configure voting, categories, roles | ✖ | ✖ | ✖ | ✖ | ✖ | ✔ |
| Approve or move external funding | ✖ | ✖ | ✖ | ✖ | ✖ | ✖ (never) |

### 4.3 Role restrictions

- Entrepreneurs and investors do not automatically receive voting privileges; an entrepreneur who is also an eligible member votes under normal member rules.
- Moderators and administrators cannot approve investments or distribute external funding.
- A user shall not vote on, or screen, their own idea. **\[Config\]** Conflict-of-interest rules for staff are defined in Section 26.
- Private conversation access is limited to participants, with exceptional access governed by a documented, audited policy (FR-G10).

### 4.4 Special roles and permissions (granted, never self-assigned)

These roles extend Section 4.1 so people with less access, skill, or confidence can still take part. Each is granted by an administrator, limited in scope, and recorded in the audit log.

| Role | Extra permission | Granted by | Limits |
| --- | --- | --- | --- |
| Idea Helper (community facilitator) | Draft and submit an idea on behalf of a person who has no device, low literacy, or needs language help | Administrator after a short vetting step | Needs the creator's recorded consent; the creator remains the owner and must confirm by SMS or email; helper cannot read the creator's private messages |
| Organization Partner (school, university, NGO, incubator) | Bulk-invite members, run a themed challenge, view aggregate statistics for their members | Administrator after organization verification | Sees only aggregates and ideas members chose to share; no private data |
| Guardian | Approve and monitor the account of a linked minor | Self-registered, linked by verified relationship | Can approve or block inquiries to the minor; cannot edit the minor's ideas |
| Language Editor / Translator | Edit translation and language of public idea text | Administrator | Cannot change meaning, status, or attribution; edits tracked in revision history |

No special role can approve, hold, or move external funding.

## 5. Module A — Public Website and Idea Discovery

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-A01 | Provide a public landing page explaining the Idea Box mission and YSSE's facilitation role. | M |
| FR-A02 | Allow visitors to browse published, publicly visible ideas without registration. | M |
| FR-A03 | Support search by title, keywords, category, and tags. | M |
| FR-A04 | Support filters: publication date, category, development stage, expected impact, popularity. | M |
| FR-A05 | Give each published idea a unique, stable, shareable URL (slug plus ID). | M |
| FR-A06 | Idea cards show title, summary, category, status, review counts, vote count where applicable, and key metadata. | M |
| FR-A07 | Drafts, rejected, restricted, and withdrawn ideas and private documents shall never appear in public discovery, search, sitemaps, or caches. | M |
| FR-A08 | Provide About, Guidelines, Privacy Policy, Terms of Use, and Contact pages. | M |
| FR-A09 | Responsive layouts for desktop, tablet, and mobile. | M |
| FR-A10 | Search results shall respect visibility and publication status at query time. | M |
| FR-A11 | Public pages shall expose Open Graph metadata that never contains anonymous creator identity or private content. | S |

**Acceptance:** A signed-out visitor can find a published idea by keyword and filter, open its URL directly, and cannot reach any unpublished idea by URL guessing (returns not-found).

## 6. Module B — Authentication and Profiles

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-B01 | Register, sign in, sign out, and recover accounts securely. | M |
| FR-B02 | Verify accounts per a configurable policy (email verification at minimum). | M |
| FR-B03 | Member profiles: display name, biography, skills, interests, optional image. | M |
| FR-B04 | Entrepreneurs and investors maintain separate professional profiles. | M |
| FR-B05 | Business profile fields: organization, industry, description, website, coarse location, interest areas. | M |
| FR-B06 | Distinguish **verified** from **self-declared** information in the UI and API. | M |
| FR-B07 | Notification preferences per event type and channel. | S |
| FR-B08 | Administrators can suspend accounts or revoke access per policy, with recorded reason. | M |
| FR-B09 | Account recovery and role changes follow secure, audited workflows. | M |
| FR-B10 | Every protected operation checks permissions on the server. | M |
| FR-B11 | Opportunity-seeker status (entrepreneur, investor, mentor) is granted through a defined approval or verification step, not self-assigned instantly. **\[Decision\]** | M |
| FR-B12 | Users can request account deletion and data export, handled per Section 22. | S |

**Acceptance:** A user cannot reach another user's private data or an admin endpoint by modifying client-side state; role elevation is logged.

### 6.1 Creator Profile

Every member can have a Creator Profile: a public-facing page that shows who they are as a problem solver, while protecting anonymous ideas. It is separate from the Entrepreneur profile (FR-B04) and can coexist with it.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-B13 | Any registered member can create a Creator Profile from a guided "Create profile" screen (3 short steps, skippable) or automatically after saving their first idea. | M |
| FR-B14 | Profile fields: public alias, headline, short bio, district or city (not exact address), languages, skills offered, help needed, areas of interest, optional links, availability, optional photo. | M |
| FR-B15 | The public alias is unique and separate from the real or legal name. The real name is never shown unless the member chooses to. | M |
| FR-B16 | Per-field visibility settings: Public, Members only, Opportunity seekers I am in conversation with, or Private. Default is the most restrictive that still lets the profile work. | M |
| FR-B17 | The public Creator page lists only published ideas with public attribution. Anonymous ideas are never listed, linked, counted, or inferable through the profile, search, "similar ideas", or leaderboards (leaderboard credit for anonymous ideas is shown to the creator only, unless they opt in). | M |
| FR-B18 | Collaboration card: skills offered, skills or support sought, preferred collaboration types. It feeds opportunity matching (Section 10.1) only for fields the creator marks as shareable. | S |
| FR-B19 | Profile highlights (published ideas, reviews received, reported outcomes, badges) are computed from persisted data and count public ideas only. | S |
| FR-B20 | Contact details (phone, email) are never public; contact happens through platform inquiries (Module F and G). | M |
| FR-B21 | A profile completeness indicator encourages but never blocks idea submission. Minimum to submit: alias and a verified contact method. | M |
| FR-B22 | Verified badges show which details are confirmed (email, phone, organization). Self-declared details are labeled as such. | S |
| FR-B23 | A member can edit or delete their Creator Profile. On deletion, public attribution becomes "Former member"; idea and audit records follow the retention policy (Section 22.3). | M |

### 6.2 Open Access and Inclusion (an Idea Box for everyone)

The Idea Box serves people with real problems and good ideas, including those with little time, money, education, or technology. These requirements make participation easy and fair.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-B24 | Free to use. No fee to browse, submit, review, or vote. | M |
| FR-B25 | Sign up with email or mobile number (one-time code). Social sign-in is optional. No national ID or real name is required to take part. | M |
| FR-B26 | Mobile-first and low-bandwidth: public pages load under 300 KB on first view, drafts autosave, uploads resume after interruption, and core flows work on low-end phones. An installable web app (PWA) is recommended. | M |
| FR-B27 | Bangla and English interface and input from day one. Idea text can be in any language; each idea carries a language tag, and translation help is available (machine suggestion plus Language Editor correction). | M |
| FR-B28 | "Quick idea" mode: three fields (title, the problem, my idea). Guided prompts and optional fields can be added later without losing the draft. | M |
| FR-B29 | Assisted submission through Idea Helpers (Section 4.4) for people with no device, low literacy, or disabilities, with recorded consent and creator confirmation. | S |
| FR-B30 | Plain-language guidelines and terms (short summaries first), with an "Is my idea safe to share?" explanation covering what to publish publicly versus share privately. | M |
| FR-B31 | Accessibility: WCAG 2.1 AA, screen-reader support, large-text option, and text alternatives for images and voice notes. | M |
| FR-B32 | Minors (13 to 17): allowed with guardian consent **\[Decision\]**. Minors' identities are never public, opportunity-seeker inquiries to minors need guardian approval, and marketplace use may be restricted. Under 13 is not permitted directly. | M |
| FR-B33 | Fairness: rankings and recommendations never use gender, religion, ethnicity, or home location as inputs. Geography is used only for voluntary matching. | M |
| FR-B34 | Ownership and credit: terms state the creator keeps ownership of their idea. The platform records a timestamped submission as proof of date, not as legal protection. | M |
| FR-B35 | Safe participation: clear reporting, block tools, and a respectful-conduct summary shown at first use. | M |

**Acceptance:** A new user on a basic phone and slow connection can register with a mobile number, save a Quick idea in Bangla, and see it appear in their dashboard; an anonymous idea is not discoverable from that creator's public profile in any way.

## 7. Module C — Idea Submission and Lifecycle

### 7.1 Submission form

| Field | Requirement |
| --- | --- |
| Idea title | Required |
| Short summary | Required |
| Problem statement | Required |
| Proposed solution | Required |
| Category | Required |
| Target audience / beneficiaries | Required |
| Expected impact | Required |
| Development stage | Required |
| Creator attribution | Required choice: public or anonymous |
| Support sought (investment, sponsorship, mentorship, partnership, technical help, other) | Optional |
| Skills / resources needed | Optional |
| Estimated budget | Optional |
| Expected timeline | Optional |
| Supporting documents or images | Optional |
| Success metrics | Optional |
| Collaboration preferences | Optional |
| Tags | Optional |

Field length limits, allowed file types, and file size caps are **\[Config\]**. Defaults: title 120 characters, summary 300, long text fields 5,000; images and PDFs up to 10 MB each, up to 5 attachments.

### 7.2 Functional requirements

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-C01 | Authorized members can create, save, edit, and submit ideas. | M |
| FR-C02 | Validate required fields and attachment restrictions server-side. | M |
| FR-C03 | Save drafts and resume later. | M |
| FR-C04 | Submitted ideas enter the configured screening process. | M |
| FR-C05 | Screening Reviewers can approve publication, request revisions, or reject, each with recorded reasons. | M |
| FR-C06 | Creators see status and feedback on their own ideas. | M |
| FR-C07 | Published ideas support community reviews, comments, eligible voting, bookmarking, and following. | M |
| FR-C08 | Suggest similar existing ideas during submission to reduce duplication. | S |
| FR-C09 | Record material edits in revision history; edits to a published idea may re-enter screening **\[Config\]**. | M |
| FR-C10 | The original creator is always recorded, even when public attribution is anonymous. | M |
| FR-C11 | Enforce only permitted status transitions (Section 7.3). | M |
| FR-C12 | Administrators configure categories and tags. | M |
| FR-C13 | Creators can withdraw an idea; withdrawn ideas leave public discovery but retain records. | S |

### 7.3 Idea status model

Statuses: **Draft, Submitted, Under Review, Revision Required, Published, Rejected, Shortlisted, Approved for Development, In Progress, Completed, Archived, Withdrawn**.

| From | To | Who can trigger |
| --- | --- | --- |
| Draft | Submitted | Creator |
| Submitted | Under Review | System / Screening Reviewer |
| Under Review | Revision Required | Screening Reviewer |
| Under Review | Published | Screening Reviewer |
| Under Review | Rejected | Screening Reviewer |
| Revision Required | Submitted | Creator (resubmit) |
| Published | Shortlisted | Screening Reviewer / Admin |
| Shortlisted | Approved for Development | Admin (decision recorded) |
| Approved for Development | In Progress | Creator / project lead |
| In Progress | Completed | Creator / project lead |
| Completed, Published, Shortlisted | Archived | Admin |
| Draft, Submitted, Revision Required, Published | Withdrawn | Creator |

Every transition records actor, timestamp, and reason. Any transition not listed is rejected by the server.

Additional rules:

- Rejected ideas are not publicly discoverable unless an authorized appeal or resubmission policy allows it.
- Publication, shortlisting, and approval for development are separate decisions.
- **Shortlisting or approval for development never implies funding approval.**
- Voting eligibility and voting windows are configured independently of publication status.

**Acceptance:** Illegal transitions (for example, Draft → Published) are rejected; every transition appears in the history with reason and actor.

### 7.4 Problem Posts and Solution Proposals

Many people know a real problem but do not have an idea yet. A Problem Post lets anyone raise it so others can propose solutions.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-C14 | Members can publish a Problem Post without a solution: title, who is affected, where (coarse location), why it matters, optional photos, urgency. | S |
| FR-C15 | Other members can respond with a "Solve this" idea, linked to the problem. The link is shown on both pages. | S |
| FR-C16 | Problem statuses: Open, Solutions proposed, In progress, Solved, Closed. Only the problem owner or a moderator changes them, with a reason. | S |
| FR-C17 | "Me too" endorsements show how many people share a problem. They are members-only, one per member, and never affect official idea voting or leaderboards. | S |
| FR-C18 | Problem Posts follow the same moderation, anonymity, and privacy rules as ideas. They must not name or identify private individuals. | M |
| FR-C19 | Opportunity seekers can browse Problem Posts and contact the problem owner or solution creators through inquiries (Module F). | C |

**Acceptance:** A member posts a problem, another member links an idea to it, the owner marks the problem Solved, and "Me too" counts do not alter the Top 3 Ideas.

## 8. Module D — Community Reviews (Guest and Registered)

This is a core requirement and shall be implemented explicitly. A community review is feedback; it is **never** a vote.

### 8.1 Access policy

| Scenario | Required behavior |
| --- | --- |
| Guest with no prior review | May submit one anonymous review on one eligible idea without signing in. |
| Recognized guest tries to review a second idea | Server refuses and prompts sign-in or registration. |
| Registered user reviews a new idea | Allowed, subject to eligibility. |
| Registered user reviews the same idea again | Existing review opens for editing; no duplicate is created. |
| Registered user edits a review | Same record updated; count unchanged; history kept per policy. |
| Guest edits their review | Allowed only if the guest visitor ID is still recognized; otherwise require sign-in or a recovery process. |
| Moderator removes a review | Excluded from public counts; only records required by audit and retention policy are kept. |
| Guest clears cookies or changes device | Cannot be reliably recognized; the guest limit is **best-effort**. |

### 8.2 Guest review requirements

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-D01 | Allow one anonymous review on one eligible idea without an account. | M |
| FR-D02 | First guest review creates a server-side guest record and an opaque guest visitor ID. | M |
| FR-D03 | Require authentication when a recognized guest attempts a review on a different idea. | M |
| FR-D04 | Display the review publicly as "Anonymous". | M |
| FR-D05 | Review counts distinguish anonymous from registered-user reviews. | M |
| FR-D06 | Never expose reviewer identity in pages, search results, APIs, metadata, or logs shown to the public. | M |
| FR-D07 | Apply rate limits, duplicate detection, and spam controls. | M |
| FR-D08 | Guest reviews never count as official votes. | M |
| FR-D09 | Clearly tell users that the guest limit is best-effort. | M |
| FR-D10 | The server decides guest eligibility; client-side checks are insufficient. | M |
| FR-D11a | Guest reviews may be held for pre-moderation **\[Config\]**; default: published immediately, subject to spam filters, with post-publication moderation. **\[Decision\]** | S |

### 8.3 Registered-user review requirements

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-D11 | Authenticated users can review multiple eligible ideas. | M |
| FR-D12 | At most one active review per user per idea. | M |
| FR-D13 | Editing never increases the review count. | M |
| FR-D14 | Prevent duplicates in backend validation **and** a database unique constraint. | M |
| FR-D15 | Registered reviews display anonymously by default; an opt-in public attribution option is a later policy change. | M |
| FR-D16 | Reviewers can edit or remove their own reviews within configured limits. | M |
| FR-D17 | Moderators can hide or remove rule-violating reviews with a recorded reason. | M |
| FR-D18 | Review moderation status (pending, published, hidden, removed, spam) is tracked separately from content. | M |
| FR-D19 | Creators cannot review their own ideas. | S |

### 8.4 Review counts

Each idea detail page shows: anonymous published reviews, registered published reviews, total published reviews, and an optional aggregate rating if ratings are enabled.

**Total published reviews = anonymous published reviews + registered published reviews.**

Only reviews with status *published* count publicly. Pending, hidden, removed, and spam reviews are excluded. Counts are computed from persisted data, not client values.

### 8.5 Guest-review technical design (initial release)

1. Issue a random, opaque guest ID server-side (at least 128 bits of entropy).
2. Store it in a cookie marked `Secure`, `HttpOnly`, and `SameSite`, with a defined lifetime **\[Config\]**.
3. Tie guest eligibility to the server-side guest record, which stores the ID (or its hash), first-review idea ID, and timestamps.
4. Before accepting a guest review, check whether the guest already reviewed a different idea.
5. Apply rate limits (for example per IP range and per guest ID), duplicate-text detection, and abuse monitoring.
6. Do not use invasive fingerprinting as a substitute for authentication.
7. Do not store personal information merely to enforce the restriction; any IP-derived data is short-lived and hashed.

**Known limitation:** guests can clear storage or change devices. If strict one-review-per-person enforcement is ever required, authentication or another reliable verification mechanism is needed. The system shall not claim guest identity is guaranteed.

**Acceptance:** Tests cover first guest review success, second-idea refusal, member duplicate prevention (including concurrent requests), edit without count change, moderation removal changing counts, and guest reviews never appearing in vote totals.

## 9. Module E — Community Voting and Rankings

Reviews and votes are separate entities with different eligibility rules.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-E01 | Only authenticated, eligible members can vote. | M |
| FR-E02 | Anonymous visitors cannot cast official votes. | M |
| FR-E03 | At most one active vote per member per idea. | M |
| FR-E04 | Prevent duplicates via backend validation and a database unique constraint. | M |
| FR-E05 | Vote change or removal follows the configured voting policy. **\[Config\]** | M |
| FR-E06 | Administrators configure voting start and end dates per voting period. | M |
| FR-E07 | Vote totals follow a configurable visibility policy (live, hidden until close, or after close). **\[Decision\]** | M |
| FR-E08 | Ineligible or fraudulent votes are excluded from official rankings, with the exclusion recorded. | M |
| FR-E09 | Results are calculated from persisted data, never from frontend values. | M |
| FR-E10 | Tie-breaking rules are published before the voting period begins (suggested order: earlier publication date, then more registered reviews). | M |
| FR-E11 | Voting configuration changes and administrative adjustments are audited. | M |
| FR-E12 | A high vote count never automatically approves an idea for investment or funding. | M |
| FR-E13 | Members cannot vote on their own ideas. | M |

**Voting eligibility (defaults, \[Config\]):** verified account, account age of at least 7 days, not suspended, idea in Published or Shortlisted status, and voting window open.

### 9.1 Top 3 Ideas

Show the three highest-ranked eligible ideas for the selected voting period, each with rank, title, eligible vote total, category, and publication status. If fewer than three exist, show only those available. Ties follow the published tie-break policy.

### 9.2 Top 3 Idea Makers

Recognize contributors with a published, configurable scoring policy. Idea count alone shall not determine rank. A recommended starting formula:

| Input | Suggested weight |
| --- | --- |
| Eligible votes received | 40% |
| Ideas progressing past screening (Published or beyond) | 25% |
| Ideas reaching Shortlisted or beyond | 15% |
| Completed projects or verified outcomes | 15% |
| Verified community contributions (quality reviews, helpful comments) | 5% |

Weights, per-input caps, and period boundaries are **\[Config\]**. The score shall exclude duplicate submissions, rejected ideas, and artificial activity, and the formula shall be visible on the leaderboard page.

**Acceptance:** Duplicate votes are rejected under concurrent requests; guest reviews never alter rankings; leaderboards match a recomputation from raw stored rows.

## 10. Module F — Entrepreneur and Investor Portal

This module is central to the YSSE intermediary model.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-F01 | Provide a dedicated portal for entrepreneurs and investors. | M |
| FR-F02 | Anyone can browse eligible public ideas without signing in. | M |
| FR-F03 | Initiating a private inquiry requires authentication as an authorized opportunity seeker. | M |
| FR-F04 | Opportunity seekers create and maintain professional profiles. | M |
| FR-F05 | Profiles include industries, interests, expertise, preferred collaboration types, and optional business information. | M |
| FR-F06 | Search and filter by category, industry, stage, expected impact, resource needs, and optional budget. | M |
| FR-F07 | Private watchlist for saved ideas. | M |
| FR-F08 | Structured inquiry to express interest in an idea. | M |
| FR-F09 | Inquiry types: investment, sponsorship, mentorship, business partnership, technical collaboration, other support. | M |
| FR-F10 | Notify the creator of each new inquiry. | M |
| FR-F11 | Creators can respond, decline, or report an inquiry. | M |
| FR-F12 | Business profiles show self-declared or verified state. | M |
| FR-F13 | Opportunity seekers never get access to private drafts, hidden creator identities, or restricted attachments. | M |
| FR-F14 | The platform never guarantees that an inquiry leads to collaboration, investment, or success. | M |
| FR-F15 | Recommendations based on stated interests and idea metadata, with explainable matching reasons. | S |
| FR-F16 | Limit inquiries per seeker per day and per idea **\[Config\]** to prevent spam. | M |

### 10.1 Opportunity matching (initial version)

Use configurable rules, not AI. Matching fields: industry and domain, category and tags, seeker skills and expertise, preferred stage, required resources, collaboration preferences, and voluntarily provided geography. Every recommendation shows *why* it matched. A match score is never a guarantee of compatibility or investment suitability.

## 11. Module G — Private Messaging and Introductions

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-G01 | Opportunity seekers initiate a private inquiry tied to a published idea. | M |
| FR-G02 | Conversations are accessible only to authorized participants. | M |
| FR-G03 | Send and receive text messages. | M |
| FR-G04 | Permitted attachments are shared through private, access-controlled storage. | S |
| FR-G05 | Messages record sender, timestamp, conversation, and delivery/read state where supported. | M |
| FR-G06 | In-app notifications and optional email alerts. | M |
| FR-G07 | Users can mute or block conversations. | M |
| FR-G08 | A block cannot be bypassed by the same account starting a new conversation. | M |
| FR-G09 | Users can report harassment, spam, fraud, or inappropriate messages. | M |
| FR-G10 | Administrators do not routinely read private conversations. Exceptional access requires documented justification, a restricted approver, and an audit entry; only reported content is shown to moderators. | M |
| FR-G11 | Enforce retention and deletion rules for messages and attachments. | M |
| FR-G12 | Anonymous creators stay anonymous unless they choose to reveal themselves; the conversation shows the public alias only. | M |
| FR-G13 | Rate limits and abuse controls for inquiries and messages. | M |
| FR-G14 | Conversation statuses: open, muted, blocked, closed. | M |

**Connection lifecycle:** Inquiry submitted → Creator notified → Response or decline → Discussion → Optional outcome report.

The platform may record that parties *report* an agreement but must never imply that YSSE approved or guaranteed it.

**Acceptance:** A user outside a conversation receives not-found or forbidden on every read, send, and attachment request, including by direct ID guessing.

## 12. Module H — External Opportunity and Funding Outcomes

This module records introductions and outcomes; it does not manage money.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-H01 | Creators specify the support they seek. | M |
| FR-H02 | Opportunity seekers initiate relevant inquiries. | M |
| FR-H03 | Optional pipeline statuses: inquiry received, discussion ongoing, proposal exchanged, agreement reported, not proceeding, outcome unknown. | S |
| FR-H04 | Status changes are visible only to authorized participants unless public reporting is explicitly approved by both parties. | M |
| FR-H05 | Parties may voluntarily report collaboration, venture, mentorship, or investment outcomes. | S |
| FR-H06 | Distinguish self-reported from independently verified information. | M |
| FR-H07 | The platform does not collect or distribute external investment money. | M |
| FR-H08 | No internal investment balance, fund approval queue, or YSSE disbursement ledger. | M |
| FR-H09 | The system never claims a financial transaction occurred unless an authorized verification process supports it. | M |
| FR-H10 | Parties remain responsible for due diligence, contracts, payments, ownership arrangements, and legal compliance; this notice appears in the UI. | M |

## 13. Module I — Community Marketplace

An independent module for reusable goods and community exchange.

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-I01 | Members list permitted items for sale, donation, or exchange. | M |
| FR-I02 | Listings include title, description, category, condition, photos, and listing type. | M |
| FR-I03 | Sale listings specify a price; donation listings state clearly they are free. | M |
| FR-I04 | Browse, search, filter, and sort listings. | M |
| FR-I05 | Buyers or recipients request a reservation. | M |
| FR-I06 | Owners accept or decline reservation requests. | M |
| FR-I07 | Listing statuses: draft, pending review, available, reserved, completed, rejected, expired, removed. | M |
| FR-I08 | Members report inappropriate or prohibited listings. | M |
| FR-I09 | Listing expiry (default 30 days **\[Config\]**) with owner notifications and renewal. | S |
| FR-I10 | The platform does not hold transaction money in the initial release. | M |
| FR-I11 | Phone numbers and addresses are never public by default. | M |
| FR-I12 | Users arrange handover and any permitted payment themselves; the UI states this. | M |
| FR-I13 | A prohibited-items policy (weapons, drugs, counterfeit goods, regulated items) is published and enforced in moderation. | M |

**Acceptance:** One reservation can be active per item at a time; accepting it moves the listing to Reserved and declines or queues competing requests.

## 14. Module J — Notifications

The platform shall generate in-app notifications (and optional email) for: idea submission and screening decisions, revision requests, new reviews and comments, voting opening and closing, leaderboard results, new inquiries, new private messages, accepted or declined collaboration requests, marketplace reservations, moderation decisions and report outcomes, and project milestone or reported-outcome updates.

- Users configure optional notifications per event type (security and account notices are always on).
- Notification text never contains sensitive message content or an anonymous creator's identity.
- Email delivery is retried on failure and unsubscribe links are honored for non-essential mail.

## 15. Module K — Administration and Moderation

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-K01 | Administrators manage accounts, roles, categories, tags, and configuration. | M |
| FR-K02 | Moderators review ideas, comments, reviews, reported messages, and marketplace listings. | M |
| FR-K03 | Moderation decisions record reasons and timestamps. | M |
| FR-K04 | Account suspension, content removal, and appeal workflows, with a defined appeal response time **\[Config\]**. | M |
| FR-K05 | Administrators configure review policies and voting periods. | M |
| FR-K06 | Sensitive administrative actions are written to an append-only audit log. | M |
| FR-K07 | Dashboards for engagement, participation, entrepreneur connections, and reported outcomes. | M |
| FR-K08 | Data exports are limited to authorized roles and are themselves audited. | M |
| FR-K09 | Administrators have no authority to approve, collect, or distribute external investment money. | M |
| FR-K10 | Privileged access to private records is limited to operational need and documented policy. | M |
| FR-K11 | Sensitive actions (role grants, voting config changes, exceptional message access) require a recorded justification. | M |

## 16. Module L — Project Collaboration and Impact Tracking

| ID | Requirement | Pri |
| --- | --- | --- |
| FR-L01 | An idea may have an optional linked project workspace. | S |
| FR-L02 | Authorized participants define objectives, deliverables, and milestones. | S |
| FR-L03 | Project leaders assign tasks, responsibilities, and deadlines. | S |
| FR-L04 | Participants publish progress updates and permitted evidence. | S |
| FR-L05 | Project status is tracked independently of idea publication and voting status. | M |
| FR-L06 | Project statuses: planned, in progress, paused, completed, cancelled. | S |
| FR-L07 | Creators report prototypes, pilots, ventures, partnerships, and measurable outcomes. | S |
| FR-L08 | Public project outcomes are published only with authorization from the relevant participants. | M |
| FR-L09 | Distinguish verified outcomes from voluntary self-reports. | M |
| FR-L10 | Private business documents are accessible only to authorized project participants. | M |

## 17. Dashboards and Reporting

**Member dashboard:** my ideas and statuses; my reviews and comments; followed and saved ideas; my voting activity; inquiries received; private conversations; collaboration invitations; marketplace listings and reservations; notifications and settings.

**Entrepreneur dashboard:** recommended and recent ideas; saved opportunities; search and filters; my inquiries and conversations; opportunity pipeline and statuses; business profile and verification status; optional outcome reports.

**Administrator dashboard:** total ideas and submission trends; published, pending, rejected, and shortlisted counts; anonymous vs registered review counts; voting activity and leaderboard stats; entrepreneur registrations and inquiry volume; response and reported-collaboration rates; marketplace and moderation statistics; abuse reports and account actions; aggregated project progress and outcomes.

**Reporting rules:** all metrics come from persisted data; every report states its time period and filters; public reports exclude anonymous identities, private conversations, and restricted information; small groups are aggregated to prevent re-identification.

## 18. Non-Functional Requirements

| ID | Category | Requirement and measurable target |
| --- | --- | --- |
| NFR-01 | Security | Server-side authentication and authorization on every protected request; input validation; abuse prevention. Zero critical or high findings open at release. |
| NFR-02 | Privacy | Protect anonymous identities, private conversations, personal data, and restricted documents; collect only data needed. |
| NFR-03 | Performance | Public pages: p95 server response under 500 ms and largest contentful paint under 2.5 s on a typical mobile connection. List endpoints paginated (default 20, max 100). |
| NFR-04 | Availability | Target 99.5% monthly uptime **\[Decision\]**, with health monitoring and alerting. |
| NFR-05 | Scalability | Designed for 50,000 registered users, 20,000 ideas, and 1 million reviews/votes without redesign; scale-up path documented. |
| NFR-06 | Accessibility | Meet WCAG 2.1 AA: keyboard navigation, contrast, labeled forms, screen-reader support. |
| NFR-07 | Responsiveness | Supported on current mobile, tablet, and desktop browsers (last two major versions). |
| NFR-08 | Maintainability | Modular code, documented APIs (OpenAPI), automated tests, consistent standards; coverage of critical business rules at least 80%. |
| NFR-09 | Auditability | Append-only logs for sensitive administrative and security events. |
| NFR-10 | Data retention | See Section 22.3 for default retention periods. |
| NFR-11 | Localization | English first; Bangla-ready (externalized strings, UTF-8, locale-aware dates and numbers). |
| NFR-12 | Recovery | Daily backups; recovery point objective 24 h or better, recovery time objective 4 h; restoration tested at least quarterly. |
| NFR-13 | Monitoring | Log errors and security events without recording message content or secrets. |

## 19. Technical Architecture

Recommended: a **modular monolith** with Clean Architecture boundaries, which suits a small team and avoids premature microservices.

**Layers:** React and TypeScript frontend → ASP.NET Core Web API → authentication and authorization → application services → domain business rules → infrastructure and repositories → PostgreSQL, private file storage, email and notification service, background processing, audit logging.

| Concern | Recommendation |
| --- | --- |
| Frontend | React, TypeScript, Tailwind CSS |
| Backend | ASP.NET Core Web API on the current .NET LTS release |
| Database | PostgreSQL with EF Core migrations |
| Authentication | ASP.NET Core Identity, role- and policy-based authorization |
| Realtime messaging | REST first; SignalR if real-time is required |
| File storage | Private object storage with short-lived signed access links |
| Background jobs | Scheduled jobs for expiry, notifications, ranking snapshots, and retention cleanup |
| Deployment | Managed hosting for frontend, API, and PostgreSQL; separate dev, test, and production environments |

**Backend modules:** Identity, Ideas, Reviews, Voting, Moderation, Entrepreneur Portal, Messaging, Marketplace, Leaderboard, Projects, Administration, Notifications, Audit. Modules communicate through defined interfaces or domain events, not by reading each other's tables directly.

## 20. Data Model and Integrity Constraints

| Entity | Responsibility |
| --- | --- |
| Users, Roles, UserRoles, Permissions | Identity and access control |
| MemberProfiles, EntrepreneurProfiles | Member and professional profiles |
| Ideas, IdeaRevisions, IdeaAttachments | Idea content, history, and file metadata |
| IdeaStatusHistory | Every status transition with actor and reason |
| IdeaScreeningDecisions | Screening decisions and feedback |
| IdeaReviews | Community review content, status, reviewer association |
| GuestVisitors | Opaque (hashed) guest ID and first-review idea |
| IdeaVotes, VotingPeriods | Official votes and voting windows |
| IdeaComments, IdeaFollows | Discussion and saves |
| IdeaCategories, Tags | Classification |
| EntrepreneurInquiries | Structured inquiries |
| Conversations, ConversationParticipants, Messages, MessageAttachments | Private messaging |
| OpportunityOutcomes | Self-reported and verified outcomes |
| Projects, ProjectMembers, Tasks, Milestones | Collaboration |
| MarketplaceListings, Reservations | Marketplace |
| Reports | Abuse reports and cases |
| Notifications | User notifications |
| AuditLogs | Administrative and security audit records |

**Required database constraints:**

- Unique (user, idea) for active reviews, and unique (user, idea, voting period) for active votes.
- Guest records enforce the best-effort one-idea limit, with a unique guest ID.
- Conversation reads and writes require a row in ConversationParticipants.
- Foreign keys on every relationship; the original creator ID on Ideas is non-null even when attribution is anonymous.
- Soft deletion with status columns for moderated content so counts can be recomputed correctly.

An opaque browser identifier is never treated as a verified human identity.

**Additional entities (v3.2):**

| Entity | Responsibility |
| --- | --- |
| CreatorProfiles | Alias, headline, bio, skills offered and sought, availability |
| ProfileFieldVisibility | Per-field visibility setting for each profile |
| ProblemPosts, ProblemEndorsements | Problems raised and "Me too" endorsements |
| IdeaProblemLinks | Links between a Problem Post and a proposed idea |
| HelperAssistedSubmissions | Idea Helper submissions with consent and creator confirmation |
| GuardianLinks | Verified guardian relationships for minors |
| OrganizationPartners | Verified partner organizations and their members |

Profile and visibility checks are enforced in the API, not the frontend. Anonymous ideas are excluded from every profile query by a database filter and tested.

## 21. API Requirements

Use versioned REST endpoints with consistent validation, pagination, error format (problem details), and authorization. Document with OpenAPI.

| Group | Example endpoints |
| --- | --- |
| Authentication | `POST /api/v1/auth/register`, `POST /api/v1/auth/login` |
| Public discovery | `GET /api/v1/ideas`, `GET /api/v1/ideas/{id}` |
| Idea management | `POST /api/v1/ideas`, `PATCH /api/v1/ideas/{id}`, `POST /api/v1/ideas/{id}/submit` |
| Reviews | `POST /api/v1/ideas/{id}/reviews`, `PATCH /api/v1/reviews/{id}`, `DELETE /api/v1/reviews/{id}` |
| Voting | `POST /api/v1/ideas/{id}/votes`, `DELETE /api/v1/ideas/{id}/votes` |
| Entrepreneur portal | `GET /api/v1/opportunities`, `GET /api/v1/entrepreneur/profile` |
| Inquiries | `POST /api/v1/ideas/{id}/inquiries`, `GET /api/v1/inquiries` |
| Messaging | `GET /api/v1/conversations`, \`GET |
| Leaderboards | `GET /api/v1/leaderboards/ideas`, `GET /api/v1/leaderboards/idea-makers` |
| Marketplace | \`GET |
| Projects | \`GET |
| Administration | `/api/v1/admin/users`, `/api/v1/admin/reports`, `/api/v1/admin/audit-logs` |

These are proposed contracts, not existing implementations. Guest-review endpoints enforce the guest policy on the server. Messaging endpoints verify conversation membership on every read, send, and attachment request. Write endpoints that create reviews, votes, and inquiries support idempotency to survive retries.

## 22. Security, Privacy, and Compliance

### 22.1 Security controls

- Authorize every protected request by role and resource.
- Prevent SQL injection, XSS, and CSRF; set secure headers and a content security policy.
- Rate-limit login, guest reviews, authenticated reviews, votes, inquiries, and messages.
- Validate uploads by type, size, and content; scan for malware; store sensitive files privately with short-lived links.
- Hash passwords with a modern algorithm; support multi-factor authentication for staff **\[S\]**.
- Protect anonymous attribution in API responses, search indexes, logs, and page metadata.
- Record privileged actions, moderation decisions, and voting configuration changes.
- Keep dev, test, and production separate; manage secrets outside source control; use HTTPS everywhere.

### 22.2 Privacy principles

Collect only what is needed; never collect personal data solely to enforce guest limits; honor access, correction, and deletion requests; explain anonymity limits plainly (the YSSE team can see the original creator internally). Legal obligations under applicable Bangladeshi data-protection and cyber-security law, and any other jurisdiction served, shall be confirmed by qualified counsel before launch **\[Decision\]**.

### 22.3 Default retention (all **\[Config\]**, confirm with counsel)

| Data | Default |
| --- | --- |
| Deleted or removed reviews and comments | Minimal audit record kept 12 months, content purged after 30 days |
| Private messages after conversation close | 24 months, then deleted |
| Guest visitor records | 12 months |
| Audit logs | 36 months |
| Rejected ideas | 12 months |
| Closed accounts | Personal data deleted within 30 days of request unless legally required |

## 23. Development Roadmap

| Phase | Scope |
| --- | --- |
| 1. Foundation | Authentication, profiles, RBAC, database schema, public website, categories, CI/CD and deployment baseline |
| 2. Core Idea Box | Idea submission, screening, anonymous attribution, guest and registered reviews, comments, voting, leaderboards |
| 3. Entrepreneur Portal | Professional profiles, discovery, filters, watchlist, structured inquiries |
| 4. Messaging and Collaboration | Private conversations, notifications, block/report, project workspaces, outcome tracking |
| 5. Marketplace | Listings, sale/donation/exchange, reservations, moderation |
| 6. Production Readiness | Security review, test hardening, performance tuning, backups, monitoring, documentation, release |

Each phase ends with a demonstrable build, passing tests for that phase's acceptance criteria, and a retrospective.

## 24. Minimum Acceptance Criteria

The first production release is not complete until all of these pass.

1. Visitors browse published ideas without registration, and cannot reach unpublished ideas by URL.
2. Members submit ideas and track screening decisions; illegal status transitions are rejected.
3. Anonymous creators' identities are not exposed in pages, APIs, metadata, notifications, or conversations.
4. A guest can submit one anonymous review on one idea.
5. A recognized guest attempting a second idea is asked to sign in.
6. Anonymous and registered review counts are calculated separately and only from published reviews.
7. Editing a review creates no duplicate and changes no count.
8. The guest restriction is documented and displayed as best-effort.
9. Guest reviews never count as votes.
10. Members cannot cast duplicate active votes, including under concurrent requests, and cannot vote on their own ideas.
11. Top 3 Ideas and Top 3 Idea Makers match a recomputation from stored data.
12. Authenticated opportunity seekers can contact creators; others cannot.
13. Private conversations are accessible only to participants.
14. Creators control disclosure of their identity.
15. Administrators moderate content without any external-funding capability.
16. Marketplace supports sale, donation, exchange, and reservation workflows.
17. Sensitive administrative actions appear in the audit log.
18. Automated tests cover authentication, authorization, review limits, voting, anonymity, and messaging.
19. Production includes HTTPS, secure configuration, backups with a tested restore, monitoring, and recovery procedures.
20. Accessibility checks (WCAG 2.1 AA) pass on core flows.

**Additional acceptance criteria (v3.2):**

21. A member can create, edit, and delete a Creator Profile, and per-field visibility is enforced for every viewer type.
22. Anonymous ideas never appear on, link from, or can be inferred from a Creator Profile, search result, or leaderboard.
23. Registration with a mobile number, the Quick idea form, and Bangla input work on a low-end phone over a slow connection.
24. Idea Helpers can submit on someone's behalf only with recorded consent, and the creator confirms ownership.
25. Minors' accounts follow guardian-consent rules, with no public identity and no unapproved inquiries.
26. Problem Posts can be created, linked to ideas, and closed, and "Me too" endorsements never change official rankings.
27. Special roles are grantable only by an administrator, are scoped, and appear in the audit log.

## 25. Explicit Exclusions

- YSSE-managed investment funds or funding distribution.
- Internal investment balances or disbursement approvals.
- Guaranteed investment matching or business success.
- Automated ownership transfer of ideas, and platform-provided intellectual property protection (creators are advised to protect their own IP before sharing sensitive details).
- Anonymous official voting.
- Payment escrow or marketplace money handling.
- Mandatory AI-based idea evaluation.
- Microservices, unless later justified operationally.

External entrepreneurs and creators remain responsible for due diligence, negotiations, agreements, and payments.

## 26. Risks and Open Decisions

| # | Item | Why it matters | Suggested default | Owner |
| --- | --- | --- | --- | --- |
| 1 | Vote visibility (live vs hidden until close) | Live totals encourage bandwagon voting | Hidden until close | YSSE |
| 2 | Voting eligibility thresholds | Prevents throwaway accounts | Verified email, 7-day age | YSSE |
| 3 | Guest reviews pre-moderated or not | Spam vs participation | Post-moderation plus spam filter | YSSE |
| 4 | How opportunity-seeker status is approved | Protects creators from bad actors | Manual approval with light verification | YSSE |
| 5 | Staff conflict-of-interest rules | Fair screening and ranking | Staff cannot screen, moderate, or vote on own ideas | YSSE |
| 6 | Idea IP and confidentiality guidance | Creators may disclose sensitive concepts | Publish guidance; encourage a public summary with details shared privately | YSSE and counsel |
| 7 | Uptime target and hosting budget | Drives architecture | 99.5% | YSSE |
| 8 | Legal and data-protection review | Launch compliance | Counsel review before Phase 2 release | YSSE |
| 9 | Guest-limit evasion | Known limitation | Accept; escalate to sign-in if abuse grows | Product |
| 10 | Marketplace safety and prohibited items | Liability and trust | Published policy and reporting | YSSE |
|  |  |  |  |  |

## 27. Final Project Definition

YSSE Idea Box is a community-powered idea-to-entrepreneurship platform. It gives people a structured place to submit ideas, gather feedback, discover opportunities, and connect privately with entrepreneurs who may help turn ideas into ventures. YSSE provides the intermediary platform and facilitation process; entrepreneurs, investors, and creators independently decide whether and how to pursue business opportunities.

**Defining features:** (1) idea submission, screening, and discovery; (2) anonymous reviews with a one-idea guest allowance and separate counts; (3) authenticated voting and transparent leaderboards; (4) entrepreneur and investor opportunity discovery; (5) private in-platform messaging; (6) optional collaboration and impact tracking; (7) community marketplace; (8) secure administration, moderation, and reporting.

Any future change shall preserve the distinction between community feedback, official voting, YSSE's facilitation role, and independently managed business opportunities.
