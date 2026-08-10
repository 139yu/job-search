(() => {
  const items = [...document.querySelectorAll('.joblist-box__item')];
  const out = items.map(i => {
    const a = i.querySelector('a.jobinfo__name');
    const name = i.querySelector('.jobinfo__name');
    const other = [...i.querySelectorAll('.jobinfo__other-info-item')].map(e => e.textContent.replace(/\s+/g, ' ').trim());
    const company = i.querySelector('.companyinfo') ? i.querySelector('.companyinfo').textContent.replace(/\s+/g, ' ').trim() : '';
    const logo = i.querySelector('.companyinfo__top a[title]');
    return {
      title: name ? name.textContent.replace(/\s+/g, ' ').trim() : '',
      href: a ? a.getAttribute('href').split('?')[0] : '',
      salary: i.querySelector('.jobinfo__salary') ? i.querySelector('.jobinfo__salary').textContent.replace(/\s+/g, ' ').trim() : '',
      other,
      company,
      companyName: logo ? logo.getAttribute('title') : ''
    };
  });
  return JSON.stringify(out);
})()
