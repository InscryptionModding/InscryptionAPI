export default {
    defaultTheme: 'dark',
    mermaid: {
        flowchart: {
            nodeSpacing: 100,
            rankSpacing: 50,
            wrappingWidth: 250,
            minNodeWidth: 180
        }
    }
}

const docUrl = document.querySelector('meta[name="docfx:docurl"]')?.content || '';

if (docUrl.includes('/api/')) {
    document.documentElement.classList.add('api-documentation');
}

if (docUrl.includes('/community/')) {
    document.documentElement.classList.add('community-documentation');
}

if (docUrl.includes('/wiki/')) {
    document.documentElement.classList.add('wiki-documentation');
}

if (docUrl.includes('/apiSpec/')) {
    document.documentElement.classList.add('apispec-documentation');
}