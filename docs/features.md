# Features & Lifecycle

Functional description of each module and the business rules that govern them. For endpoint details see [api-reference.md](api-reference.md).

---

## 1. Ideas

Creators submit social-impact ideas either **fully** (title, summary, category, problem, solution, audience, impact, stage, support sought, budget, timeline, metrics, collaboration preferences, tags) or **quickly** (title, problem, solution — designed for low-bandwidth/low-literacy users; a default category and `Concept` stage are applied).

- Ideas support **tags** (max 8), **attachments**, and **view counts**.
- Every edit creates an **`IdeaRevision`** snapshot (content, editor, change note, incrementing revision number).
- Ideas can be **followed** and commented on.
- **Similar ideas** are suggested based on category/tags/keywords.
- **Attribution** is `Public` (creator shown) or `Anonymous` (creator hidden publicly but still recorded internally and excluded from profiles/leaderboards/search inference).
- Drafts can be deleted; user-facing content uses **soft delete**.

### Validation

| Field | Min | Max |
| --- | --- | --- |
| Title | 3 | 120 |
| Short summary | 10 | 300 |
| Problem statement | 10 | 5000 |
| Proposed solution | 10 | 5000 |
| Target audience | 3 | 500 |
| Expected impact | 3 | 500 |
| Each tag | 1 | 30 |

Configurable via `PlatformSettings` (`idea.maxTags`, `idea.titleMaxLength`, `idea.summaryMaxLength`, `idea.longTextMaxLength`, `idea.maxAttachments`, `idea.maxAttachmentSizeMB`).

---

## 2. Idea lifecycle

`IdeaStatusMachine` enforces the transition matrix; `IdeaStatusHistory` records every change (actor, reason, timestamp).

```text
Draft → Submitted → UnderReview → Published → Shortlisted → ApprovedForDevelopment → InProgress → Completed → Archived
                       │              │
                       ├─ Rejected ───┴─► Archived
                       └─ RevisionRequired → Submitted
                                        │
                    Published/Submitted/etc. → Withdrawn → Archived
```

**Allowed transitions**

| From | To |
| --- | --- |
| Draft | Submitted, Withdrawn |
| Submitted | UnderReview, Withdrawn |
| UnderReview | Published, Rejected, RevisionRequired |
| RevisionRequired | Submitted, Withdrawn |
| Published | Shortlisted, Archived, Withdrawn, UnderReview *(on edit)* |
| Shortlisted | ApprovedForDevelopment, Completed, Archived |
| ApprovedForDevelopment | InProgress, Archived |
| InProgress | Completed, Archived |
| Completed | Archived |
| Rejected | Archived |
| Withdrawn | Archived |

**Rules**
- The creator may edit only in `Draft`, `RevisionRequired`, and `Published`.
- Editing a `Published` idea re-enters `UnderReview` and creates a revision snapshot.
- Illegal transitions are rejected server-side (`Result.Fail` / `409`).
- Published ideas are the only ones publicly visible; `PublishedAt` is stamped on publication.

---

## 3. Screening & publication

Reviewers work a queue and record decisions.

- **Queue** filters by status (defaults to `Submitted`, `UnderReview`, `RevisionRequired`); supports search and pagination.
- **Decisions:** `Published`, `Rejected`, `RevisionRequired` (legacy verbs `Approve`/`Reject`/`RequestRevision` accepted).
- A decision on a `Submitted` idea first moves it to `UnderReview`, then applies the decision (two-step, validated by the status machine).
- Decisions are recorded in `IdeaScreeningDecisions` with rationale; the creator is notified; significant events are audited (`ScreeningDecided`, `IdeaStatusChanged`).

---

## 4. Reviews & comments

- Registered users and **guests** can review ideas with a rating and text.
- Guest reviews are tracked via an HTTP-only guest cookie and rate-limited (`guest-review`, 3/hour).
- Reviews have a moderation status (`Pending`, `Approved`, `Rejected`, `Flagged`); moderators can change it.
- Threaded comments attach to ideas; authors can edit/delete their own; moderators can moderate.
- Users can follow ideas and view their followed list.

---

## 5. Voting & leaderboards

- **Votes** are cast per user per idea; eligibility and uniqueness are enforced server-side and in the database.
- Vote visibility is configurable (`Public` or aggregate-only).
- **Voting periods** can be created, activated, and closed; a period scopes voting to a window.
- **Leaderboards** rank ideas and idea-makers using votes, reviews, and verified outcomes. A **monthly** leaderboard can be announced.
- Leaderboards/dashboard aggregates use Dapper for single-statement efficiency.

---

## 6. Problems

Members post **problem statements** that ideas can address.

- Endorsements accumulate support; status moves through `Open → InReview → Endorsed → Linked → Resolved → Archived`.
- Ideas can be **linked** to problems (`IdeaProblemLink`).
- Moderation changes status; posting is permission-gated.

---

## 7. Entrepreneurship

- **Profiles** for entrepreneurs, investors, and mentors (with field-level visibility).
- **Inquiries** (`Mentorship`, `Investment`, `Partnership`, `Feedback`, `Other`) link an interested party to an idea; owners can respond and update.
- **Watchlists** let users track ideas.
- **Matching** computes idea↔opportunity-seeker fit via `MatchingScorer` (category, stage, support sought, skills).

---

## 8. Messaging

- 1:1 **conversations** with participants, messages, and attachments.
- Read state per participant; conversation status (`Active`, `Archived`, etc.).
- **Blocking** prevents unwanted contact; **reporting** feeds moderation.
- Rate-limited (`messaging`, 60/min). Only participants can read a conversation.

---

## 9. Outcomes

- Records real-world results of ideas (`OpportunityOutcome`) such as collaborations or investments.
- Outcomes have a verification status (`Pending`, `Verified`, `Rejected`); verified outcomes boost credibility and leaderboard scores.

---

## 10. Projects

Projects turn an approved idea into structured work.

- **Create** requires a live idea (not Draft/Rejected/Withdrawn/Archived/RevisionRequired) and the creator to be a participant with entrepreneur/investor status (admins bypass).
- **Members, milestones, tasks, and updates** are managed by project owners.
- Project status: `Planning`, `Active`, `OnHold`, `Completed`, `Archived`.

---

## 11. Marketplace

- Listings of type `Sale`, `Donation`, or `Exchange`, with status `Draft → Active → Reserved → Sold/Withdrawn → Archived`.
- Buyers create **reservations**; owners respond (`Accepted`/`Declined`/`Cancelled`/`Completed`).
- Public browsing; management is permission-gated.

---

## 12. Notifications

- **In-app** notifications plus optional **email** delivery, with per-user **preferences** (channel/type).
- Delivered in real time via **SignalR** (`/hubs/notifications`) and persisted so they survive reload.
- Users can mark one/all read and delete.

---

## 13. Profiles

- Member, creator, and entrepreneur profiles with configurable **field visibility**.
- Avatars and media served through the media endpoint.
- Entrepreneur profiles can require approval (`/profiles/entrepreneur/pending`, `/approve`).
- Public creator profiles expose only published, publicly-attributed ideas.

---

## 14. Administration

| Area | Capabilities |
| --- | --- |
| Users | List/create/update/delete, suspend/reactivate, manage roles. |
| Roles & Permissions | CRUD roles and permissions, assign permissions to roles, view role members. |
| Categories & Tags | CRUD. |
| Screening | Queue and decisions (via screening access). |
| Content moderation | Reviews, comments, problems, reports, featured ideas. |
| Voting | Voting periods and leaderboard announcements. |
| Static content | CMS-managed pages/snippets. |
| Platform settings | Key/value configuration (see below). |
| Audit log | Review recorded administrative/state changes. |
| Dashboard | Counts and time-series metrics. |

### Platform settings (examples)

| Key | Default | Meaning |
| --- | --- | --- |
| `idea.maxAttachments` | 5 | Max files per idea. |
| `idea.maxAttachmentSizeMB` | 10 | Attachment size limit. |
| `idea.titleMaxLength` | 120 | Title limit. |
| `idea.summaryMaxLength` | 300 | Summary limit. |
| `idea.longTextMaxLength` | 5000 | Long-text limit. |
| `idea.maxTags` | 8 | Max tags. |

---

## 15. Cross-cutting rules

- **Pagination:** server-side, default 20, max 100.
- **Auditing:** significant changes recorded with actor/reason.
- **Soft delete:** content is retained with `IsDeleted`/`DeletedAt`.
- **Rate limiting:** per-policy as described in [security.md](security.md#4-rate-limiting--abuse-control).

---

## See also

- [API Reference](api-reference.md) · [Backend](backend.md) · [Security](security.md)
