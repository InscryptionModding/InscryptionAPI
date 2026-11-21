const getContentWidth = () => {
    const article = document.querySelector('article');

    if (!article) {
        return 0;
    }

    const style = getComputedStyle(article);

    return article.clientWidth
        - parseFloat(style.paddingLeft || 0)
        - parseFloat(style.paddingRight || 0);
};

const getNodeWidth = () => Math.max(180, Math.floor((getContentWidth() - 40) / 2));

const options = {
    defaultTheme: 'dark',

    // docfx does `const { mermaid } = await options()` then Object.assign()s it
    // into initialize(), so these getters run once per render — the first
    // render included. No patching, no ordering race.
    get mermaid() {
        const width = getNodeWidth();

        return {
            flowchart: {
                minNodeWidth: width,
                wrappingWidth: width
            }
        };
    },

    start: () => {
        const article = document.querySelector('article');

        if (!article || !window.ResizeObserver) {
            return;
        }

        const MERMAID_FALLBACK =
            'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

        let mermaidPromise;

        // 'mermaid' is a bare specifier and there is no import map, so resolve
        // the chunk docfx already loaded instead. Cached after first success.
        const loadMermaid = () => {
            mermaidPromise ??= (async () => {
                const chunk = performance
                    .getEntriesByType('resource')
                    .map(entry => entry.name)
                    .find(url => /\/mermaid[.\-][\w\-]*\.min\.js$/.test(url));

                return (await import(chunk || MERMAID_FALLBACK)).default;
            })().catch(error => {
                mermaidPromise = null;
                throw error;
            });

            return mermaidPromise;
        };

        let lastWidth = article.clientWidth;
        let timer = 0;
        let running = false;

        const render = async () => {
            const articleWidth = article.clientWidth;

            if (running || articleWidth === lastWidth) {
                return;
            }

            lastWidth = articleWidth;

            // docfx has not converted the code blocks yet; let it finish.
            if (article.querySelector('pre code.lang-mermaid')) {
                timer = setTimeout(render, 250);
                return;
            }

            const nodes = [];

            article.querySelectorAll('pre.mermaid[data-mermaid]').forEach(diagram => {
                if (!diagram.offsetParent) {
                    return;
                }

                diagram.removeAttribute('data-processed');
                diagram.innerHTML = diagram.getAttribute('data-mermaid');

                nodes.push(diagram);
            });

            if (nodes.length === 0) {
                return;
            }

            running = true;

            try {
                const mermaid = await loadMermaid();
                const theme = document.documentElement.getAttribute('data-bs-theme') === 'dark'
                    ? 'dark'
                    : 'default';

                mermaid.initialize({
                    ...options.mermaid,
                    startOnLoad: false,
                    theme
                });

                await mermaid.run({ nodes });
            } catch (error) {
                console.error(error);
            } finally {
                running = false;
            }
        };

        new ResizeObserver(() => {
            clearTimeout(timer);
            timer = setTimeout(render, 150);
        }).observe(article);
    }
};

export default options;

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