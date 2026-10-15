const express = require('express');
const router = express.Router();

// Almacenamiento en memoria (simplificado)
let orders = [];
let nextOrderId = 1;

// Crear orden de servicio
// body: { customer, description }
router.post('/', (req, res) => {
  const { customer, description } = req.body || {};
  if (!customer || !description) {
    return res.status(400).json({ error: 'customer y description son obligatorios' });
  }

  const order = {
    id: nextOrderId++,
    customer,
    description,
    assignedTo: null,
    status: 'pendiente', // pendiente, en_curso, completado
    createdAt: new Date().toISOString()
  };
  orders.push(order);
  return res.status(201).json(order);
});

// Listar órdenes
router.get('/', (req, res) => {
  return res.json(orders);
});

// Obtener orden por id
router.get('/:id', (req, res) => {
  const id = Number(req.params.id);
  const order = orders.find(o => o.id === id);
  if (!order) return res.status(404).json({ error: 'Orden no encontrada' });
  return res.json(order);
});

// Actualizar orden (editar, asignar técnico, cambiar estado)
// body puede incluir: description, assignedTo, status
router.put('/:id', (req, res) => {
  const id = Number(req.params.id);
  const order = orders.find(o => o.id === id);
  if (!order) return res.status(404).json({ error: 'Orden no encontrada' });

  const { description, assignedTo, status } = req.body || {};
  if (description !== undefined) order.description = description;
  if (assignedTo !== undefined) order.assignedTo = assignedTo;
  if (status !== undefined) {
    const valid = ['pendiente', 'en_curso', 'completado'];
    if (!valid.includes(status)) return res.status(400).json({ error: 'Estado inválido' });
    order.status = status;
  }

  order.updatedAt = new Date().toISOString();
  return res.json(order);
});

module.exports = router;
