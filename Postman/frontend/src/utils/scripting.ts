/**
 * Postman'ning `pm.*` API'siga o'xshash, lekin qisqartirilgan skript muhiti.
 * Pre-request/test scriptlar Electron renderer'ning o'z JS kontekstida (Chromium V8)
 * bajariladi — bu to'liq OS-darajasidagi sandbox emas, ichki tool uchun yetarli.
 */

export interface TestResult {
  name: string
  passed: boolean
  error?: string
}

function fmt(value: unknown): string {
  try {
    return JSON.stringify(value)
  } catch {
    return String(value)
  }
}

export class Expectation<T> {
  private negate = false
  private actual: T

  constructor(actual: T) {
    this.actual = actual
  }

  get not() {
    this.negate = true
    return this
  }
  get to() {
    return this
  }
  get be() {
    return this
  }
  get have() {
    return this
  }
  get exist() {
    this.assert(this.actual !== null && this.actual !== undefined, 'qiymat mavjud bo\'lishi kerak edi')
    return this
  }
  get true() {
    this.assert((this.actual as unknown) === true, `${fmt(this.actual)} true bo'lishi kerak edi`)
    return this
  }
  get false() {
    this.assert((this.actual as unknown) === false, `${fmt(this.actual)} false bo'lishi kerak edi`)
    return this
  }

  private assert(cond: boolean, msg: string) {
    if (this.negate ? cond : !cond) throw new Error(msg)
  }

  equal(expected: unknown) {
    this.assert(this.actual === expected, `${fmt(this.actual)} qiymati ${fmt(expected)}ga teng bo'lishi kerak edi`)
    return this
  }
  eql(expected: unknown) {
    this.assert(fmt(this.actual) === fmt(expected), `${fmt(this.actual)} qiymati ${fmt(expected)}ga (deep) teng bo'lishi kerak edi`)
    return this
  }
  a(type: string) {
    this.assert(typeof this.actual === type, `${fmt(this.actual)} turi "${type}" bo'lishi kerak edi`)
    return this
  }
  above(n: number) {
    this.assert((this.actual as unknown as number) > n, `${fmt(this.actual)} ${n}dan katta bo'lishi kerak edi`)
    return this
  }
  below(n: number) {
    this.assert((this.actual as unknown as number) < n, `${fmt(this.actual)} ${n}dan kichik bo'lishi kerak edi`)
    return this
  }
  include(item: unknown) {
    const actual = this.actual as unknown
    const cond = Array.isArray(actual) ? actual.includes(item) : typeof actual === 'string' && actual.includes(String(item))
    this.assert(cond, `${fmt(this.actual)} ${fmt(item)}ni o'z ichiga olishi kerak edi`)
    return this
  }
}

export interface RuntimeVars {
  get(key: string): string | undefined
  set(key: string, value: string): void
}

export function createPreRequestPm(vars: RuntimeVars) {
  return {
    environment: vars,
    variables: vars,
  }
}

export interface ScriptResponse {
  code: number
  status: string
  responseTime: number
  headers: Record<string, string>
  text: () => string
  json: () => unknown
}

export function createTestPm(response: ScriptResponse, results: TestResult[]) {
  return {
    response,
    test(name: string, fn: () => void) {
      try {
        fn()
        results.push({ name, passed: true })
      } catch (e) {
        results.push({ name, passed: false, error: e instanceof Error ? e.message : String(e) })
      }
    },
    expect<T>(actual: T) {
      return new Expectation(actual)
    },
  }
}

/** Skriptni bajaradi; xato tashlasa (pm.test ichida emas, umumiy skriptda), matnini qaytaradi. */
export function runScript(script: string, pm: unknown): { error?: string } {
  if (!script?.trim()) return {}
  try {
    const fn = new Function('pm', script)
    fn(pm)
    return {}
  } catch (e) {
    return { error: e instanceof Error ? e.message : String(e) }
  }
}
