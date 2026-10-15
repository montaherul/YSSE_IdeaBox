import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Mail, MessageSquare } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { requiredText, optionalText } from '@/lib/validation'
import { StaticPage } from '@/features/static/components/StaticBlock'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'

const contactSchema = z.object({
  name: requiredText(2, 120, 'Name'),
  email: z.email('Enter a valid email address'),
  message: requiredText(10, 2000, 'Message'),
  subject: optionalText(160, 'Subject'),
})

export default function ContactPage() {
  useDocumentTitle('Contact')
  const form = useForm({ resolver: zodResolver(contactSchema), defaultValues: { name: '', email: '', subject: '', message: '' } })

  const onSubmit = () => {
    form.reset()
  }

  return (
    <StaticPage
      key="contact"
      title="Contact us"
      description="Questions, feedback or need help? Send us a message."
      fallbackSections={
        <div className="mx-auto max-w-2xl space-y-6">
          <Card>
            <CardContent className="p-6">
              <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
                <FormField id="contact-name" label="Your name" required error={form.formState.errors.name?.message}>
                  <Input id="contact-name" autoComplete="name" {...form.register('name')} />
                </FormField>
                <FormField id="contact-email" label="Email address" required error={form.formState.errors.email?.message}>
                  <Input id="contact-email" type="email" autoComplete="email" {...form.register('email')} />
                </FormField>
                <FormField id="contact-subject" label="Subject (optional)" error={form.formState.errors.subject?.message}>
                  <Input id="contact-subject" {...form.register('subject')} />
                </FormField>
                <FormField id="contact-message" label="Message" required error={form.formState.errors.message?.message}>
                  <Textarea id="contact-message" rows={5} {...form.register('message')} />
                </FormField>
                <Button type="submit" className="w-full sm:w-auto">
                  <MessageSquare className="h-4 w-4" aria-hidden="true" />
                  Send message
                </Button>
              </form>
            </CardContent>
          </Card>

          <p className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
            <Mail className="h-4 w-4" aria-hidden="true" />
            Prefer email? Reach the team at hello@ysse.example. See our{' '}
            <Link to="/guidelines" className="text-primary hover:underline">
              community guidelines
            </Link>
            .
          </p>
        </div>
      }
    />
  )
}
