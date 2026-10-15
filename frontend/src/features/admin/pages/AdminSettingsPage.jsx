import { useState } from 'react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAdminSettings, useUpdateAdminSettings } from '@/features/admin/hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Separator } from '@/components/ui/separator'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'

import { toast } from 'sonner'

const SETTING_GROUPS = {
  general: [
    { key: 'Site.Name', label: 'Site Name', type: 'text', placeholder: 'YSSE Idea Box' },
    { key: 'Site.Tagline', label: 'Tagline', type: 'text', placeholder: 'Think boldly. Build together.' },
    { key: 'Site.Kicker', label: 'Kicker', type: 'text', placeholder: 'A community for what\'s next' },
    { key: 'Site.Description', label: 'Meta Description', type: 'textarea', rows: 3 },
    { key: 'Site.Keywords', label: 'SEO Keywords (comma-separated)', type: 'text' },
  ],
  theme: [
    { key: 'Theme.PrimaryColor', label: 'Primary Color', type: 'text', placeholder: '#29b77d' },
    { key: 'Theme.SecondaryColor', label: 'Secondary Color', type: 'text', placeholder: '#6b7280' },
    { key: 'Theme.AccentColor', label: 'Accent Color', type: 'text', placeholder: '#f59e0b' },
    { key: 'Theme.DarkModePrimary', label: 'Dark Mode Primary', type: 'text', placeholder: '#34c98c' },
  ],
  features: [
    { key: 'Feature.GuestReviewsEnabled', label: 'Enable Guest Reviews', type: 'boolean' },
    { key: 'Feature.ProblemPostsEnabled', label: 'Enable Problem Posts', type: 'boolean' },
    { key: 'Feature.MarketplaceEnabled', label: 'Enable Marketplace', type: 'boolean' },
    { key: 'Feature.ProjectsEnabled', label: 'Enable Projects', type: 'boolean' },
    { key: 'Feature.VotingEnabled', label: 'Enable Voting', type: 'boolean' },
    { key: 'Feature.MessagingEnabled', label: 'Enable Messaging', type: 'boolean' },
  ],
  limits: [
    { key: 'Limit.MaxIdeaAttachments', label: 'Max Idea Attachments', type: 'number', min: 0, max: 20 },
    { key: 'Limit.MaxAttachmentSizeMB', label: 'Max Attachment Size (MB)', type: 'number', min: 1, max: 100 },
    { key: 'Limit.MaxTagsPerIdea', label: 'Max Tags Per Idea', type: 'number', min: 1, max: 20 },
    { key: 'Limit.MaxGuestReviewLength', label: 'Max Guest Review Length', type: 'number', min: 100, max: 10000 },
    { key: 'Limit.MaxRegisteredReviewLength', label: 'Max Registered Review Length', type: 'number', min: 100, max: 10000 },
    { key: 'Limit.DailyInquiryLimit', label: 'Daily Inquiry Limit Per Seeker', type: 'number', min: 1, max: 100 },
    { key: 'Limit.MarketplaceListingExpiryDays', label: 'Marketplace Listing Expiry (days)', type: 'number', min: 1, max: 365 },
    { key: 'Limit.GuestReviewCookieDays', label: 'Guest Review Cookie Expiry (days)', type: 'number', min: 1, max: 730 },
  ],
  moderation: [
    { key: 'Moderation.ReviewPolicy', label: 'Review Policy', type: 'select', options: ['manual_moderation', 'auto_publish', 'pre_moderation'] },
    { key: 'Moderation.AutoHideReportThreshold', label: 'Auto-hide Report Threshold', type: 'number', min: 1, max: 50 },
    { key: 'Moderation.RequireEmailVerification', label: 'Require Email Verification', type: 'boolean' },
    { key: 'Moderation.RequirePhoneVerification', label: 'Require Phone Verification', type: 'boolean' },
  ],
  boundary: [
    { key: 'Boundary.Notice', label: 'Business Boundary Notice', type: 'textarea', rows: 4 },
  ],
}

export default function AdminSettingsPage() {
  useDocumentTitle('Website Settings')
  const { data: settings, isLoading } = useAdminSettings()
  const update = useUpdateAdminSettings()

  const [pendingChanges, setPendingChanges] = useState({})
  const [activeTab, setActiveTab] = useState('general')
  const [isSaving, setIsSaving] = useState(false)

  const handleChange = (key, value) => {
    setPendingChanges(prev => ({ ...prev, [key]: value }))
  }

  const saveAll = async () => {
    setIsSaving(true)
    try {
      for (const [key, value] of Object.entries(pendingChanges)) {
        await update.mutateAsync({ key, value: String(value) })
      }
      toast.success('All settings saved successfully')
      setPendingChanges({})
    } catch (err) {
      toast.error('Failed to save some settings')
    } finally {
      setIsSaving(false)
    }
  }

  const getValue = (key, defaultValue = '') => {
    if (pendingChanges[key] !== undefined) return pendingChanges[key]
    return settings?.[key] ?? defaultValue
  }

  const renderInput = (setting) => {
    const value = getValue(setting.key, setting.defaultValue ?? '')
    const onChange = (e) => handleChange(setting.key, setting.type === 'number' ? (e.target.value ? Number(e.target.value) : '') : 
      setting.type === 'boolean' ? e.target.checked : e.target.value)

    switch (setting.type) {
      case 'textarea':
        return (
          <FormField key={setting.key} id={setting.key} label={setting.label}>
            <Textarea
              id={setting.key}
              rows={setting.rows ?? 3}
              value={value}
              onChange={onChange}
              placeholder={setting.placeholder}
            />
          </FormField>
        )
      case 'select':
        return (
          <FormField key={setting.key} id={setting.key} label={setting.label}>
            <select
              id={setting.key}
              value={value}
              onChange={onChange}
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {setting.options.map(opt => <option key={opt} value={opt}>{opt}</option>)}
            </select>
          </FormField>
        )
      case 'boolean':
        return (
          <div key={setting.key} className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label htmlFor={setting.key}>{setting.label}</Label>
            </div>
            <Switch id={setting.key} checked={Boolean(value)} onCheckedChange={checked => handleChange(setting.key, checked)} />
          </div>
        )
      case 'number':
        return (
          <FormField key={setting.key} id={setting.key} label={setting.label}>
            <Input
              type="number"
              id={setting.key}
              min={setting.min}
              max={setting.max}
              value={value}
              onChange={onChange}
              className="w-32"
            />
          </FormField>
        )
      default:
        return (
          <FormField key={setting.key} id={setting.key} label={setting.label}>
            <Input
              id={setting.key}
              type="text"
              value={value}
              onChange={onChange}
              placeholder={setting.placeholder}
            />
          </FormField>
        )
    }
  }

  if (isLoading) return <PageSkeleton lines={8} />

  return (
    <div className="space-y-6">
      <PageHeader
        title="Website Settings"
        description="Manage all site configuration, branding, features and limits."
        actions={
          <Button onClick={saveAll} disabled={isSaving || Object.keys(pendingChanges).length === 0}>
            {isSaving ? 'Saving…' : 'Save all changes'}
          </Button>
        }
      />

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="general">General</TabsTrigger>
          <TabsTrigger value="theme">Theme</TabsTrigger>
          <TabsTrigger value="features">Features</TabsTrigger>
          <TabsTrigger value="limits">Limits</TabsTrigger>
          <TabsTrigger value="moderation">Moderation</TabsTrigger>
          <TabsTrigger value="boundary">Boundary</TabsTrigger>
        </TabsList>

        {Object.entries(SETTING_GROUPS).map(([groupKey, settingsList]) => (
          <TabsContent key={groupKey} value={groupKey} className="pt-4">
            <Card>
              <CardHeader>
                <CardTitle className="capitalize">{groupKey} Settings</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                  {settingsList.map(setting => renderInput(setting))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        ))}
      </Tabs>

      {Object.keys(pendingChanges).length > 0 && (
        <div className="fixed bottom-4 right-4 z-50 bg-primary text-primary-foreground px-4 py-2 rounded-lg shadow-lg animate-slide-in">
          {Object.keys(pendingChanges).length} unsaved change(s)
        </div>
      )}
    </div>
  )
}