const rows = document.querySelector('#medicineRows'), search = document.querySelector('#search');
const medicineDialog = document.querySelector('#medicineDialog'), saleDialog = document.querySelector('#saleDialog');
let saleId = null;
const daysUntil = date => Math.ceil((new Date(date).setHours(0,0,0,0) - new Date().setHours(0,0,0,0)) / 86400000);
const money = value => new Intl.NumberFormat('en-IN',{style:'currency',currency:'INR'}).format(value);
function flash(message) { const t=document.querySelector('#toast'); t.textContent=message; t.classList.add('show'); setTimeout(()=>t.classList.remove('show'),2800); }
async function load() {
  const response = await fetch(`/api/medicines?search=${encodeURIComponent(search.value)}`); const medicines = await response.json();
  rows.innerHTML = medicines.map(m => { const expiring=daysUntil(m.expiryDate)<30; const low=m.quantity<10; const cls=expiring?'expiry':low?'low':''; return `<tr class="${cls}"><td><strong>${escapeHtml(m.fullName)}</strong></td><td>${escapeHtml(m.brand)}</td><td>${new Date(m.expiryDate).toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'})}</td><td>${m.quantity}</td><td>${money(m.price)}</td><td><button class="sale" data-id="${m.id}" data-name="${escapeHtml(m.fullName)}" data-stock="${m.quantity}">Record sale</button></td><td><button class="delete" data-id="${m.id}" data-name="${escapeHtml(m.fullName)}">Delete</button></td></tr>`; }).join('');
  document.querySelector('#empty').hidden=medicines.length>0;
  document.querySelector('#medicineCount').textContent=medicines.length;
  document.querySelector('#lowStockCount').textContent=medicines.filter(m=>m.quantity<10).length;
  document.querySelector('#expiryCount').textContent=medicines.filter(m=>daysUntil(m.expiryDate)<30).length;
}
function escapeHtml(v) { const el=document.createElement('div'); el.textContent=v; return el.innerHTML; }
document.querySelector('#newMedicine').onclick=()=>{document.querySelector('#medicineForm').reset(); document.querySelector('#medicineError').textContent=''; medicineDialog.showModal();};
document.querySelectorAll('[data-close]').forEach(b=>b.onclick=()=>b.closest('dialog').close());
search.oninput=()=>load();
rows.onclick = async event => {
  const saleButton = event.target.closest('.sale');
  if (saleButton) {
    saleId = saleButton.dataset.id;
    document.querySelector('#saleMedicine').textContent = `${saleButton.dataset.name} - ${saleButton.dataset.stock} in stock`;
    document.querySelector('#saleForm').reset();
    document.querySelector('#saleError').textContent = '';
    saleDialog.showModal();
    return;
  }

  const deleteButton = event.target.closest('.delete');
  if (!deleteButton || !confirm(`Delete ${deleteButton.dataset.name}? This cannot be undone.`)) return;

  const response = await fetch(`/api/medicines/${deleteButton.dataset.id}`, { method: 'DELETE' });
  if (!response.ok) {
    flash('The medicine could not be deleted.');
    return;
  }

  flash(`${deleteButton.dataset.name} was deleted.`);
  load();
};
document.querySelector('#medicineForm').onsubmit = async event => {
  event.preventDefault();

  const form = event.currentTarget;
  const saveButton = document.querySelector('#saveMedicine');
  const errorMessage = document.querySelector('#medicineError');
  const formData = new FormData(form);
  const medicine = Object.fromEntries(formData);
  medicine.quantity = Number(medicine.quantity);
  medicine.price = Number(medicine.price);

  // Disabling the button prevents a quick double-click from sending two requests.
  saveButton.disabled = true;
  saveButton.textContent = 'Saving...';
  errorMessage.textContent = '';

  try {
    const response = await fetch('/api/medicines', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(medicine)
    });

    if (!response.ok) {
      const error = await response.json();
      errorMessage.textContent = error.message || 'Please check all required values.';
      return;
    }

    medicineDialog.close();
    flash('Medicine added successfully.');
    load();
  } finally {
    saveButton.disabled = false;
    saveButton.textContent = 'Save medicine';
  }
};
document.querySelector('#saleForm').onsubmit=async e=>{e.preventDefault();const quantity=+new FormData(e.target).get('quantity');const r=await fetch(`/api/medicines/${saleId}/sales`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({quantity})});if(!r.ok){document.querySelector('#saleError').textContent=(await r.json()).message || 'Could not save sale.';return;}saleDialog.close();flash('Sale recorded and stock updated.');load();};
load();
