(() => {
  const cards = [...document.querySelectorAll('li.job-card-box')];
  const out = cards.map(c => ({
    title: c.querySelector('.job-name') ? c.querySelector('.job-name').textContent.trim() : '',
    salary: c.querySelector('.salary, .job-salary') ? (c.querySelector('.salary, .job-salary').textContent || '').trim() : '',
    info: c.querySelector('.job-info') ? c.querySelector('.job-info').textContent.replace(/\s+/g, ' ').trim() : '',
    tags: [...c.querySelectorAll('.tags, .tag-list')].map(t => t.textContent.replace(/\s+/g, ' ').trim()).join('|'),
    company: c.querySelector('.company-name, .name, .company-text, .boss-name') ? (c.querySelector('.company-name, .name, .company-text, .boss-name').textContent || '').trim() : '',
    area: c.querySelector('.job-area, .job-location, .company-location') ? (c.querySelector('.job-area, .job-location, .company-location').textContent || '').trim() : '',
    text: c.innerText.replace(/\s+/g, ' ').trim()
  }));
  return JSON.stringify(out);
})()
