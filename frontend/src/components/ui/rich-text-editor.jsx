import { useEffect } from 'react'
import { useEditor, EditorContent } from '@tiptap/react'
import StarterKit from '@tiptap/starter-kit'
import { cn } from '@/lib/utils'
import { sanitizeHtml } from '@/lib/sanitize'

function ToolbarButton({ label, shortcut, active, disabled, onClick }) {
  return (
    <button
      type="button"
      title={`${label} (${shortcut})`}
      aria-pressed={active}
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className={cn(
        'h-7 min-w-[1.75rem] rounded-md px-1.5 text-sm font-medium transition-colors',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
        active ? 'bg-accent text-accent-foreground' : 'hover:bg-accent/60',
      )}
    >
      {label}
    </button>
  )
}

/**
 * TipTap rich-text editor producing sanitized HTML. Keyboard accessible
 * (WCAG): every toolbar control is a real button with a label and shortcut.
 */
export function RichTextEditor({ value = '', onChange, placeholder = 'Write here…', disabled = false }) {
  const editor = useEditor({
    extensions: [StarterKit.configure({ heading: { levels: [1, 2, 3] } })],
    content: value || '',
    editable: !disabled,
    onUpdate: ({ editor: instance }) => onChange?.(instance.getHTML()),
    editorProps: {
      attributes: {
        class:
          'prose prose-sm max-w-none min-h-[160px] px-3 py-2 focus:outline-none',
        'aria-label': placeholder,
      },
    },
  })

  useEffect(() => {
    if (editor && value !== editor.getHTML()) {
      editor.commands.setContent(value || '', { emitUpdate: false })
    }
  }, [value, editor])

  if (!editor) return null

  const actions = [
    { label: 'B', shortcut: 'Ctrl+B', active: editor.isActive('bold'), onClick: () => editor.chain().focus().toggleBold().run() },
    { label: 'I', shortcut: 'Ctrl+I', active: editor.isActive('italic'), onClick: () => editor.chain().focus().toggleItalic().run() },
    { label: 'S', shortcut: 'Ctrl+Shift+X', active: editor.isActive('strike'), onClick: () => editor.chain().focus().toggleStrike().run() },
    { label: 'H1', shortcut: 'Ctrl+Alt+1', active: editor.isActive('heading', { level: 1 }), onClick: () => editor.chain().focus().toggleHeading({ level: 1 }).run() },
    { label: 'H2', shortcut: 'Ctrl+Alt+2', active: editor.isActive('heading', { level: 2 }), onClick: () => editor.chain().focus().toggleHeading({ level: 2 }).run() },
    { label: 'H3', shortcut: 'Ctrl+Alt+3', active: editor.isActive('heading', { level: 3 }), onClick: () => editor.chain().focus().toggleHeading({ level: 3 }).run() },
    { label: '• List', shortcut: 'Ctrl+Shift+8', active: editor.isActive('bulletList'), onClick: () => editor.chain().focus().toggleBulletList().run() },
    { label: '1. List', shortcut: 'Ctrl+Shift+7', active: editor.isActive('orderedList'), onClick: () => editor.chain().focus().toggleOrderedList().run() },
    { label: 'Quote', shortcut: 'Ctrl+Shift+B', active: editor.isActive('blockquote'), onClick: () => editor.chain().focus().toggleBlockquote().run() },
    { label: 'Code', shortcut: 'Ctrl+Alt+C', active: editor.isActive('codeBlock'), onClick: () => editor.chain().focus().toggleCodeBlock().run() },
  ]

  return (
    <div className="rounded-md border border-input shadow-sm focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-1">
      <div className="flex flex-wrap items-center gap-0.5 border-b bg-muted/40 px-1 py-1" role="toolbar" aria-label="Formatting">
        {actions.map((action) => (
          <ToolbarButton key={action.label} {...action} disabled={disabled} />
        ))}
      </div>
      <EditorContent editor={editor} />
    </div>
  )
}

/** Sanitized HTML renderer for CMS/content blocks. */
export function RichText({ html, className, id }) {
  if (!html) return null
  return (
    <div id={id} className={cn('prose max-w-none', className)} dangerouslySetInnerHTML={{ __html: sanitizeHtml(html) }} />
  )
}