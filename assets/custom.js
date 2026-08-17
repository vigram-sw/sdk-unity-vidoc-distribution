(function () {
  function relativeRoot() {
    var depth = window.location.pathname.split('/').filter(Boolean).length - 1;
    return depth > 0 ? '../'.repeat(depth) : './';
  }

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    try {
      localStorage.setItem('vigram-docs-theme', theme);
    } catch (_) {
      // Ignore storage failures in private browsing modes.
    }
  }

  function setupTheme() {
    var saved = null;
    try {
      saved = localStorage.getItem('vigram-docs-theme');
    } catch (_) {
      saved = null;
    }

    applyTheme(saved || 'light');

    var button = document.querySelector('.theme-toggle') || document.createElement('button');
    button.type = 'button';
    button.className = 'theme-toggle';
    button.setAttribute('aria-label', 'Switch light and dark theme');
    button.textContent = document.documentElement.getAttribute('data-theme') === 'dark' ? 'Light' : 'Dark';
    button.addEventListener('click', function () {
      var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      applyTheme(next);
      button.textContent = next === 'dark' ? 'Light' : 'Dark';
    });

    if (!button.parentNode) {
      var header = document.querySelector('.navbar .navbar-header');
      var brand = document.querySelector('.navbar .navbar-brand');
      var nav = document.querySelector('.navbar .container') || document.querySelector('.navbar');
      if (brand && brand.parentNode) {
        brand.parentNode.insertBefore(button, brand.nextSibling);
      } else if (header) {
        header.appendChild(button);
      } else if (nav) {
        nav.appendChild(button);
      }
    }
  }

  function copyText(text, done) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(done, done);
      return;
    }

    var textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    try {
      document.execCommand('copy');
    } finally {
      document.body.removeChild(textarea);
      done();
    }
  }

  function setupCopyButtons() {
    document.querySelectorAll('pre').forEach(function (pre) {
      if (pre.querySelector('.copy-code-button')) {
        return;
      }

      var wrapper = document.createElement('div');
      wrapper.className = 'code-copy-wrapper';
      pre.parentNode.insertBefore(wrapper, pre);
      wrapper.appendChild(pre);

      var button = document.createElement('button');
      button.type = 'button';
      button.className = 'copy-code-button';
      button.textContent = 'Copy';
      button.setAttribute('aria-label', 'Copy code block');
      button.addEventListener('click', function () {
        copyText(pre.innerText, function () {
          button.textContent = 'Copied';
          window.setTimeout(function () {
            button.textContent = 'Copy';
          }, 1200);
        });
      });
      wrapper.appendChild(button);
    });
  }

  function setup() {
    setupTheme();
    setupCopyButtons();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', setup);
  } else {
    setup();
  }
})();
