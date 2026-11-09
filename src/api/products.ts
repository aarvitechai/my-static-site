// Thin wrapper around fetch for the backend's /api/products endpoints.
// VITE_API_URL is empty in dev (Vite proxy handles /api); set it for a deployed backend.
const BASE = `${import.meta.env.VITE_API_URL ?? ''}/api/products`

export interface Product {
  id: number
  name: string
  price: number
  createdAt: string
}

export type ProductInput = Pick<Product, 'name' | 'price'>

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  return res.status === 204 ? (undefined as T) : res.json()
}

export const getProducts = () => request<Product[]>(BASE)

export const createProduct = (input: ProductInput) =>
  request<Product>(BASE, { method: 'POST', body: JSON.stringify(input) })

export const deleteProduct = (id: number) =>
  request<void>(`${BASE}/${id}`, { method: 'DELETE' })
