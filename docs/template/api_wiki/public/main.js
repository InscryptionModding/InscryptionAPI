export default {
    defaultTheme: 'dark',

    start: () => {
        const article = document.querySelector('article');

        if (!article || !window.ResizeObserver) {
            return;
        }

        import('mermaid').then(({ default: mermaid }) => {
            const originalInitialize = mermaid.initialize.bind(mermaid);
            const originalRun = mermaid.run.bind(mermaid);

            let lastConfig = mermaid.getConfig();
            let lastWidth = article.clientWidth;

            const getMermaidConfig = (config) => {
                const articleWidth = article.clientWidth;

                const targetNodeWidth = Math.max(
                    180,
                    Math.floor((articleWidth - 40) / 2)
                );

                return {
                    ...config,
                    flowchart: {
                        ...config.flowchart,
                        minNodeWidth: targetNodeWidth,
                        wrappingWidth: targetNodeWidth
                    }
                };
            };

            mermaid.initialize = (config = {}) => {
                const result = originalInitialize(getMermaidConfig(config));
            
                lastConfig = mermaid.getConfig();
            
                return result;
            };

            const resizeObserver = new ResizeObserver(() => {
                const articleWidth = article.clientWidth;

                if (articleWidth === lastWidth) {
                    return;
                }

                lastWidth = articleWidth;

                const diagrams = article.querySelectorAll(
                    'pre.mermaid[data-mermaid]'
                );

                const nodes = [];

                diagrams.forEach(diagram => {
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

                originalInitialize(getMermaidConfig(lastConfig));
                lastConfig = mermaid.getConfig();
                originalRun({ nodes });
            });

            resizeObserver.observe(article);
        });
    }
};

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