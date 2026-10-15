export function PageHeader({ title, description, leading, actions, className }) {
  return (
    <div className={`flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between ${className ?? ''}`}>
      <div className="flex min-w-0 items-center gap-3">
        {leading ? <div className="flex shrink-0 items-center gap-2">{leading}</div> : null}
        <div className="min-w-0 space-y-1">
          <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
          {description ? <p className="text-sm text-muted-foreground">{description}</p> : null}
        </div>
      </div>
      {actions ? <div className="flex flex-wrap items-center gap-2">{actions}</div> : null}
    </div>
  )
}

export default PageHeader
