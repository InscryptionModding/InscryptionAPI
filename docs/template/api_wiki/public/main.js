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

// These reach .nodeLabel/.edgeLabel and change measured label size, so a
// re-render is required even though the article width stays put.
const LAYOUT_STYLE_VARS = ['--code-line-height', '--code-font-size', '--code-font'];
const LAYOUT_ATTRS = ['data-code-font'];

const readLayoutSignature = () => {
    const root = document.documentElement;
    const style = root.style;

    return [
        ...LAYOUT_STYLE_VARS.map(name => style.getPropertyValue(name)),
        ...LAYOUT_ATTRS.map(name => root.getAttribute(name) || '')
    ].join('|');
};

const options = {
    defaultTheme: 'dark',

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
        let lastSignature = readLayoutSignature();
        let timer = 0;
        let pendingForce = false;
        let running = false;

        const schedule = force => {
            pendingForce ||= force;
            clearTimeout(timer);

            timer = setTimeout(() => {
                const forced = pendingForce;
                pendingForce = false;
                render(forced);
            }, 150);
        };

        const render = async force => {
            if (running) {
                schedule(force);
                return;
            }

            const articleWidth = article.clientWidth;

            if (!force && articleWidth === lastWidth) {
                return;
            }

            lastWidth = articleWidth;

            // docfx has not converted the code blocks yet; let it finish.
            if (article.querySelector('pre code.lang-mermaid')) {
                schedule(force);
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

        new ResizeObserver(() => schedule(false)).observe(article);

        new MutationObserver(() => {
            const signature = readLayoutSignature();

            if (signature === lastSignature) {
                return;
            }

            lastSignature = signature;
            schedule(true);
        }).observe(document.documentElement, {
            attributes: true,
            attributeFilter: ['style', ...LAYOUT_ATTRS]
        });

        // DPComic is a webfont; measurements taken before it loads are wrong.
        document.fonts?.ready.then(() => schedule(true));
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