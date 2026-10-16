import { lazy, Suspense } from 'react'
import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom'
import { Providers } from './providers'
import { PublicLayout } from '@/layouts/PublicLayout'
import { WorkspaceLayout } from '@/layouts/WorkspaceLayout'
import { ProtectedRoute, AdminRoute } from '@/layouts/ProtectedRoute'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'

// Lazy-loaded routes (rule.md §4: admin dashboard + marketplace load lazily).
const wrapLazy = (factory) => {
  const Component = lazy(factory)
  return (
    <Suspense
      fallback={
        <div className="p-4">
          <PageSkeleton lines={4} />
        </div>
      }
    >
      <Component />
    </Suspense>
  )
}

// Per-route guard (role access first, then permission access).
const guard = (element, { roles, permissions } = {}) => (
  <ProtectedRoute roles={roles} permissions={permissions}>
    {element}
  </ProtectedRoute>
)

// ---- Lazy page references ----
const MarketplacePage = () => wrapLazy(() => import('@/features/marketplace/pages/MarketplacePage.jsx'))
const ListingDetailPage = () => wrapLazy(() => import('@/features/marketplace/pages/ListingDetailPage.jsx'))
const NewListingPage = () => wrapLazy(() => import('@/features/marketplace/pages/NewListingPage.jsx'))
const MyListingsPage = () => wrapLazy(() => import('@/features/marketplace/pages/MyListingsPage.jsx'))
const EditListingPage = () => wrapLazy(() => import('@/features/marketplace/pages/EditListingPage.jsx'))

const AdminDashboardPage = () => wrapLazy(() => import('@/features/admin/pages/AdminDashboardPage.jsx'))
const UsersPage = () => wrapLazy(() => import('@/features/users/pages/UsersPage.jsx'))
const AdminModerationPage = () => wrapLazy(() => import('@/features/admin/pages/AdminModerationPage.jsx'))
const AdminCategoriesPage = () => wrapLazy(() => import('@/features/admin/pages/AdminCategoriesPage.jsx'))
const AdminVotingPage = () => wrapLazy(() => import('@/features/admin/pages/AdminVotingPage.jsx'))
const AdminAuditPage = () => wrapLazy(() => import('@/features/admin/pages/AdminAuditPage.jsx'))
const AdminSettingsPage = () => wrapLazy(() => import('@/features/admin/pages/AdminSettingsPage.jsx'))
const AdminTagsPage = () => wrapLazy(() => import('@/features/admin/pages/AdminTagsPage.jsx'))
const AdminContentPage = () => wrapLazy(() => import('@/features/admin/pages/AdminContentPage.jsx'))
const AdminRolesPage = () => wrapLazy(() => import('@/features/admin/pages/AdminRolesPage.jsx'))
const AdminFeaturedPage = () => wrapLazy(() => import('@/features/admin/pages/AdminFeaturedPage.jsx'))
const AdminEntrepreneursPage = () => wrapLazy(() => import('@/features/admin/pages/AdminEntrepreneursPage.jsx'))
const AdminPermissionsPage = () => wrapLazy(() => import('@/features/admin/pages/AdminPermissionsPage.jsx'))

export const router = createBrowserRouter([
  {
    element: (
      <Providers>
        <PublicLayout />
      </Providers>
    ),
    children: [
      { path: '/', element: <LandingPage /> },
      { path: '/ideas', element: <DiscoverPage /> },
      { path: '/ideas/:slug', element: <IdeaDetailPage /> },
      { path: '/categories/:slug', element: <CategoryPage /> },
      { path: '/discover', element: <DiscoverPage /> },
      { path: '/problems', element: <ProblemsPage /> },
      { path: '/problems/:id', element: <ProblemDetailPage /> },
      { path: '/problems/new', element: <NewProblemPage /> },
      { path: '/leaderboard', element: <LeaderboardsPage /> },
      { path: '/leaderboards', element: <LeaderboardsPage /> },
      { path: '/marketplace', element: <MarketplacePage /> },
      { path: '/marketplace/:id', element: <ListingDetailPage /> },
      { path: '/creator/:userId', element: <CreatorProfilePage /> },
      { path: '/about', element: <AboutPage /> },
      { path: '/how-it-works', element: <HowItWorksPage /> },
      { path: '/guidelines', element: <GuidelinesPage /> },
      { path: '/privacy', element: <PrivacyPage /> },
      { path: '/terms', element: <TermsPage /> },
      { path: '/contact', element: <ContactPage /> },
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
      { path: '/forgot-password', element: <ForgotPasswordPage /> },
      { path: '/reset-password', element: <ResetPasswordPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  {
    element: (
      <Providers>
        <ProtectedRoute />
      </Providers>
    ),
    children: [
      {
        element: <WorkspaceLayout />,
        children: [
          { path: '/app', element: <DashboardPage /> },
          { path: '/app/ideas', element: <MyIdeasPage /> },
          { path: '/app/submissions/new', element: <IdeaFormPage /> },
          { path: '/app/notifications', element: <NotificationsPage /> },
          { path: '/app/profile', element: <SettingsPage /> },
          { path: '/dashboard', element: <DashboardPage /> },
          { path: '/ideas/new', element: <IdeaFormPage /> },
          { path: '/ideas/:id/edit', element: <IdeaEditPage /> },
          { path: '/me/ideas', element: <MyIdeasPage /> },
          { path: '/me/reviews', element: <MyReviewsPage /> },
          { path: '/me/following', element: <FollowingPage /> },
          { path: '/me/settings', element: <SettingsPage /> },
          { path: '/messages', element: <MessagesPage /> },
          { path: '/messages/:id', element: <ConversationPage /> },
          { path: '/notifications', element: <NotificationsPage /> },
          { path: '/opportunities', element: <EntrepreneurPortalPage /> },
          { path: '/opportunities/:slug', element: <IdeaDetailPage /> },
          { path: '/inbox', element: <MessagesPage /> },
          { path: '/inbox/:conversationId', element: <ConversationPage /> },
          { path: '/entrepreneur', element: <EntrepreneurPortalPage /> },
          { path: '/entrepreneur/profile', element: <EntrepreneurProfilePage /> },
          { path: '/creator/profile', element: <CreatorProfileEditorPage /> },
          { path: '/inquiries', element: <InquiriesPage /> },
          { path: '/problems/:id/edit', element: <EditProblemPage /> },
          { path: '/marketplace/new', element: <NewListingPage /> },
          { path: '/marketplace/mine', element: <MyListingsPage /> },
          { path: '/marketplace/:id/edit', element: <EditListingPage /> },
          { path: '/projects/new', element: guard(<ProjectCreatePage />, { permissions: ['projects.create'] }) },
          { path: '/projects/:id/edit', element: <EditProjectPage /> },
          { path: '/projects/:id', element: <ProjectPage /> },
          {
            element: (
              <AdminRoute>
                <Outlet />
              </AdminRoute>
            ),
            children: [
              { path: '/review/queue', element: <ReviewQueuePage /> },
              { path: '/review/assignments/:ideaId', element: <ReviewAssignmentPage /> },
            ],
          },
        ],
      },
    ],
  },
  {
    element: (
      <Providers>
        <ProtectedRoute />
      </Providers>
    ),
    children: [
      {
        element: <WorkspaceLayout />,
        children: [
          { path: '/admin', element: guard(<AdminDashboardPage />, { permissions: ['dashboard.read'] }) },
          { path: '/users', element: guard(<UsersPage />, { permissions: ['users.read'] }) },
          { path: '/admin/users', element: <Navigate to="/users" replace /> },
          { path: '/admin/roles', element: guard(<AdminRolesPage />, { permissions: ['roles.read'] }) },
          { path: '/admin/permissions', element: guard(<AdminPermissionsPage />, { permissions: ['permissions.read'] }) },
      { path: '/admin/entrepreneurs', element: guard(<AdminEntrepreneursPage />, { roles: ['admin'] }) },
      { path: '/admin/categories', element: guard(<AdminCategoriesPage />, { roles: ['admin'] }) },
      { path: '/admin/tags', element: guard(<AdminTagsPage />, { roles: ['admin'] }) },
          { path: '/admin/content', element: guard(<AdminContentPage />, { permissions: ['staticcontent.read'] }) },
          { path: '/admin/voting', element: guard(<AdminVotingPage />, { permissions: ['votingperiods.read'] }) },
          { path: '/admin/featured', element: guard(<AdminFeaturedPage />, { permissions: ['ideas.feature'] }) },
          { path: '/admin/projects', element: guard(<AdminProjectArchivePage />, { permissions: ['projects.manage'] }) },
          { path: '/admin/sessions', element: guard(<AdminVotingPage />, { permissions: ['votingperiods.read'] }) },
          { path: '/admin/audit', element: guard(<AdminAuditPage />, { permissions: ['audit.read'] }) },
          { path: '/admin/reports', element: guard(<AdminAuditPage />, { permissions: ['audit.read'] }) },
          { path: '/admin/settings', element: guard(<AdminSettingsPage />, { permissions: ['settings.read'] }) },
          {
            path: '/admin/moderation',
            element: guard(<AdminModerationPage />, { roles: ['moderator', 'reviewer'], permissions: ['content.reports.read'] }),
          },
          {
            path: '/admin/ideas',
            element: guard(<AdminModerationPage />, { roles: ['moderator', 'reviewer'], permissions: ['content.reports.read'] }),
          },
        ],
      },
    ],
  },
])

// Lightweight layer so main.jsx stays minimal.

function LandingPage() {
  return wrapLazy(() => import('@/features/home/pages/LandingPage.jsx'))
}
function DiscoverPage() {
  return wrapLazy(() => import('@/features/ideas/pages/DiscoverPage.jsx'))
}
function CategoryPage() {
  return wrapLazy(() => import('@/features/ideas/pages/CategoryDetailPage.jsx'))
}
function IdeaDetailPage() {
  return wrapLazy(() => import('@/features/ideas/pages/IdeaDetailPage.jsx'))
}
function CreatorProfilePage() {
  return wrapLazy(() => import('@/features/profile/pages/CreatorProfilePage.jsx'))
}
function ProblemsPage() {
  return wrapLazy(() => import('@/features/problems/pages/ProblemsPage.jsx'))
}
function ProblemDetailPage() {
  return wrapLazy(() => import('@/features/problems/pages/ProblemDetailPage.jsx'))
}
function NewProblemPage() {
  return wrapLazy(() => import('@/features/problems/pages/NewProblemPage.jsx'))
}
function EditProblemPage() {
  return wrapLazy(() => import('@/features/problems/pages/EditProblemPage.jsx'))
}
function LeaderboardsPage() {
  return wrapLazy(() => import('@/features/voting/pages/LeaderboardsPage.jsx'))
}
function AboutPage() {
  return wrapLazy(() => import('@/features/static/pages/AboutPage.jsx'))
}
function HowItWorksPage() {
  return wrapLazy(() => import('@/features/static/pages/HowItWorksPage.jsx'))
}
function GuidelinesPage() {
  return wrapLazy(() => import('@/features/static/pages/GuidelinesPage.jsx'))
}
function PrivacyPage() {
  return wrapLazy(() => import('@/features/static/pages/PrivacyPage.jsx'))
}
function TermsPage() {
  return wrapLazy(() => import('@/features/static/pages/TermsPage.jsx'))
}
function ContactPage() {
  return wrapLazy(() => import('@/features/static/pages/ContactPage.jsx'))
}
function NotFoundPage() {
  return wrapLazy(() => import('@/features/static/pages/NotFoundPage.jsx'))
}
function LoginPage() {
  return wrapLazy(() => import('@/features/auth/pages/LoginPage.jsx'))
}
function RegisterPage() {
  return wrapLazy(() => import('@/features/auth/pages/RegisterPage.jsx'))
}
function ForgotPasswordPage() {
  return wrapLazy(() => import('@/features/auth/pages/ForgotPasswordPage.jsx'))
}
function ResetPasswordPage() {
  return wrapLazy(() => import('@/features/auth/pages/ResetPasswordPage.jsx'))
}
function DashboardPage() {
  return wrapLazy(() => import('@/features/dashboard/pages/DashboardPage.jsx'))
}
function IdeaFormPage() {
  return wrapLazy(() => import('@/features/ideas/pages/IdeaFormPage.jsx'))
}
function IdeaEditPage() {
  return wrapLazy(() => import('@/features/ideas/pages/IdeaEditPage.jsx'))
}
function MyIdeasPage() {
  return wrapLazy(() => import('@/features/ideas/pages/MyIdeasPage.jsx'))
}
function MyReviewsPage() {
  return wrapLazy(() => import('@/features/reviews/pages/MyReviewsPage.jsx'))
}
function FollowingPage() {
  return wrapLazy(() => import('@/features/reviews/pages/FollowingPage.jsx'))
}
function SettingsPage() {
  return wrapLazy(() => import('@/features/profile/pages/SettingsPage.jsx'))
}
function MessagesPage() {
  return wrapLazy(() => import('@/features/messaging/pages/MessagesPage.jsx'))
}
function ConversationPage() {
  return wrapLazy(() => import('@/features/messaging/pages/ConversationPage.jsx'))
}
function NotificationsPage() {
  return wrapLazy(() => import('@/features/notifications/pages/NotificationsPage.jsx'))
}
function EntrepreneurPortalPage() {
  return wrapLazy(() => import('@/features/entrepreneur/pages/EntrepreneurPortalPage.jsx'))
}
function EntrepreneurProfilePage() {
  return wrapLazy(() => import('@/features/profile/pages/EntrepreneurProfilePage.jsx'))
}
function CreatorProfileEditorPage() {
  return wrapLazy(() => import('@/features/profile/pages/CreatorProfileEditorPage.jsx'))
}
function InquiriesPage() {
  return wrapLazy(() => import('@/features/entrepreneur/pages/InquiriesPage.jsx'))
}
function ProjectPage() {
  return wrapLazy(() => import('@/features/projects/pages/ProjectPage.jsx'))
}
function ProjectCreatePage() {
  return wrapLazy(() => import('@/features/projects/pages/ProjectCreatePage.jsx'))
}
function EditProjectPage() {
  return wrapLazy(() => import('@/features/projects/pages/EditProjectPage.jsx'))
}
function AdminProjectArchivePage() {
  return wrapLazy(() => import('@/features/projects/pages/AdminProjectArchivePage.jsx'))
}
function ReviewQueuePage() {
  return wrapLazy(() => import('@/features/admin/pages/ReviewQueuePage.jsx'))
}
function ReviewAssignmentPage() {
  return wrapLazy(() => import('@/features/admin/pages/ReviewAssignmentPage.jsx'))
}