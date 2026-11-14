export default {
    defaultTheme: 'dark'
}

const docUrl = document.querySelector('meta[name="docfx:docurl"]')?.content || '';

if (docUrl.includes('/docs/api/')) {
    document.documentElement.classList.add('api-documentation');
}

if (docUrl.includes('/docs/community/')) {
    document.documentElement.classList.add('community-documentation');
}

if (docUrl.includes('/docs/wiki/')) {
    document.documentElement.classList.add('wiki-documentation');
}