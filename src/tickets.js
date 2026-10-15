const express = require('express');
const router = express.Router();

// Almacenamiento en memoria para tickets y eventos simples
let tickets = [];
let nextTicketId = 1;

// Simulación de catálogo de eventos
const events = [
  { id: 1, name: 'Concierto A', price: 30 },
  { id: 2, name: 'Concierto B', price: 45 }
];

// Comprar tickets (pago simulado)
// body: { eventId, quantity, buyer }
router.post('/purchase', (req, res) => {
  const { eventId, quantity, buyer } = req.body || {};
  if (!eventId || !quantity || !buyer) return res.status(400).json({ error: 'eventId, quantity y buyer son obligatorios' });

  const event = events.find(e => e.id === Number(eventId));
  if (!event) return res.status(404).json({ error: 'Evento no encontrado' });

  const total = event.price * Number(quantity);

  // Pago simulado: siempre aprobado
  const ticket = {
    id: nextTicketId++,
    eventId: event.id,
    eventName: event.name,
    quantity: Number(quantity),
    buyer,
    total,
    paid: true,
    purchasedAt: new Date().toISOString()
  };

  tickets.push(ticket);
  return res.status(201).json({ ticket, message: 'Pago simulado: aprobado' });
});

// Listar compras
router.get('/', (req, res) => res.json(tickets));

// Obtener catálogo de eventos
router.get('/events', (req, res) => res.json(events));

module.exports = router;
