(() => {
  const cards = [...document.querySelectorAll('.joblist-item-job-wrapper')];
  const out = cards.map(c => {
    const j = c.querySelector('.joblist-item-job');
    let entity = null;
    try {
      entity = JSON.parse(j.getAttribute('sensorsdata').split('&quot;').join('"'));
    } catch (e) {}
    const titleEl = c.querySelector('.joblist-item-job a, .jtitle, .joblist-item-job__name, a');
    return {
      title: titleEl ? titleEl.textContent.trim() : '',
      name: c.querySelector('.cname') ? c.querySelector('.cname').textContent.trim() : '',
      company: c.querySelector('.bc') ? c.querySelector('.bc').textContent.replace(/\s+/g, ' ').trim() : '',
      tags: [...c.querySelectorAll('.tag')].map(t => t.textContent.trim()).join('|'),
      entity
    };
  });
  return JSON.stringify(out);
})()
