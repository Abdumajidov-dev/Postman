export function resolveTemplate(raw: string, resolve: (key: string) => string | undefined): string {
  return raw.replace(/{{\s*(\w+)\s*}}/g, (match, key: string) => resolve(key) ?? match)
}
