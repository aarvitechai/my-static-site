import { useEffect, useState, type FormEvent } from 'react'
import { createProduct, deleteProduct, getProducts, type Product } from '../api/products'

export default function Products() {
  const [products, setProducts] = useState<Product[]>([])
  const [error, setError] = useState<string | null>(null)
  const [name, setName] = useState('')
  const [price, setPrice] = useState('')

  const load = () =>
    getProducts()
      .then((data) => {
        setProducts(data)
        setError(null)
      })
      .catch((e: Error) => setError(`Could not reach the API (${e.message}). Is the backend running?`))

  useEffect(() => {
    load()
  }, [])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    try {
      await createProduct({ name, price: Number(price) })
      setName('')
      setPrice('')
      await load()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  const handleDelete = async (id: number) => {
    await deleteProduct(id)
    await load()
  }

  return (
    <section style={{ padding: 24, textAlign: 'left', maxWidth: 520, margin: '0 auto' }}>
      <h2>Products (from .NET API)</h2>
      {error && <p style={{ color: 'crimson' }}>{error}</p>}

      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} required minLength={2} />
        <input placeholder="Price" type="number" step="0.01" min="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
        <button type="submit">Add</button>
      </form>

      <ul>
        {products.map((p) => (
          <li key={p.id} style={{ marginBottom: 6 }}>
            {p.name} — ${p.price.toFixed(2)}{' '}
            <button type="button" onClick={() => handleDelete(p.id)}>Delete</button>
          </li>
        ))}
      </ul>
    </section>
  )
}
