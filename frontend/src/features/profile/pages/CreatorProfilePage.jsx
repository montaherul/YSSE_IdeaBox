import { useParams, Link } from 'react-router-dom'
import { MapPin, Globe, Wrench, HelpCircle, Sparkles, CalendarClock } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useCreatorProfileByUser } from '@/features/profile/hooks'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { UserAvatar } from '@/components/shared/UserAvatar'

export default function CreatorProfilePage() {
  const { userId } = useParams()
  const { data: profile, isLoading, isError, error } = useCreatorProfileByUser(userId)
  const title = profile?.alias ? `${profile.alias} — Creator` : 'Creator profile'
  useDocumentTitle(title)

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />
  if (!profile) return <ErrorState title="Profile not found" message="This creator has not published a public profile." />

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div className="flex flex-col items-center gap-4 rounded-xl border bg-card p-6 text-center sm:flex-row sm:text-left">
        <UserAvatar name={profile.alias ?? profile.displayName} src={profile.photoUrl} className="h-20 w-20 text-2xl" />
        <div className="min-w-0 flex-1">
          <h1 className="text-2xl font-bold tracking-tight">{profile.alias ?? profile.displayName}</h1>
          {profile.headline ? <p className="text-base text-primary">{profile.headline}</p> : null}
          {profile.district ? (
            <p className="mt-1 inline-flex items-center gap-1 text-sm text-muted-foreground">
              <MapPin className="h-4 w-4" aria-hidden="true" />
              {profile.district}
            </p>
          ) : null}
        </div>
      </div>

      {profile.shortBio ? (
        <Card>
          <CardHeader>
            <CardTitle>About</CardTitle>
          </CardHeader>
          <CardContent className="whitespace-pre-line text-sm text-muted-foreground">{profile.shortBio}</CardContent>
        </Card>
      ) : null}

      <div className="grid gap-6 sm:grid-cols-2">
        {profile.languages ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm">
                <Globe className="h-4 w-4" aria-hidden="true" />
                Languages
              </CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground">{profile.languages}</CardContent>
          </Card>
        ) : null}

        {profile.skillsOffered ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm">
                <Wrench className="h-4 w-4" aria-hidden="true" />
                Skills offered
              </CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground">{profile.skillsOffered}</CardContent>
          </Card>
        ) : null}

        {profile.helpNeeded ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm">
                <HelpCircle className="h-4 w-4" aria-hidden="true" />
                Help needed
              </CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground">{profile.helpNeeded}</CardContent>
          </Card>
        ) : null}

        {profile.areasOfInterest ? (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-sm">
                <Sparkles className="h-4 w-4" aria-hidden="true" />
                Areas of interest
              </CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground">{profile.areasOfInterest}</CardContent>
          </Card>
        ) : null}
      </div>

      {profile.availability || profile.links ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-sm">
              <CalendarClock className="h-4 w-4" aria-hidden="true" />
              Availability &amp; links
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm text-muted-foreground">
            {profile.availability ? <p>{profile.availability}</p> : null}
            {profile.links ? (
              <a href={profile.links} target="_blank" rel="noreferrer" className="inline-block break-all text-primary hover:underline">
                {profile.links}
              </a>
            ) : null}
          </CardContent>
        </Card>
      ) : null}

      <p className="text-sm text-muted-foreground">
        <Link to="/ideas" className="text-primary hover:underline">
          Browse ideas from this creator on the Discover page.
        </Link>
      </p>
    </div>
  )
}