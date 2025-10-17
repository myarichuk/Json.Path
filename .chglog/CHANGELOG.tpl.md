# Changelog



{{ range .Versions }}

## {{ if .Tag.Previous -}}

\[{{ .Tag.Name }}]({{ $.Info.RepositoryURL }}/compare/{{ .Tag.Previous.Name }}...{{ .Tag.Name }}) - {{ datetime "2006-01-02" .Tag.Date }}
{{- else -}}
{{ .Tag.Name }} - {{ datetime "2006-01-02" .Tag.Date }}
{{- end }}

{{ if .CommitGroups }}
{{ range .CommitGroups }}
{{- if ne .Title "Chores" }}

### {{ .Title }}

{{ range .Commits }}

* {{ if .Scope }}**{{ .Scope }}:** {{ end }}{{ .Subject }} (\[{{ .Hash.Short }}]({{ $.Info.RepositoryURL }}/commit/{{ .Hash.Long }}))
  {{ end }}
  {{ end }}
  {{ end }}
  {{ end }}

{{ if .NoteGroups }}

### ⚠️ Breaking Changes

{{ range .NoteGroups }}
{{ range .Notes }}

* {{ .Body }}
  {{ end }}
  {{ end }}
  {{ end }}

{{ if .MergeCommits }}

### Merges

{{ range .MergeCommits }}

* {{ .Header }}
  {{ end }}
  {{ end }}

{{ end }}

