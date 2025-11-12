window.addEventListener('DOMContentLoaded', function () {
  const toggleBtn = document.querySelector('.admin-sidebar-toggle');
  const sidebar = document.querySelector('.admin-sidebar');

  if (toggleBtn && sidebar) {
    toggleBtn.addEventListener('click', function () {
      sidebar.classList.toggle('sidebar-open');
    });
  }

  // Close sidebar when clicking outside on mobile
  document.addEventListener('click', function (event) {
    if (!sidebar || !toggleBtn || window.innerWidth >= 1200) {
      return;
    }
    if (!sidebar.contains(event.target) && !toggleBtn.contains(event.target)) {
      sidebar.classList.remove('sidebar-open');
    }
  });

  // Custom confirm modal
  const confirmBackdrop = document.getElementById('adminConfirmModal');
  const confirmMessage = document.getElementById('adminConfirmMessage');
  const confirmOk = document.getElementById('adminConfirmOk');
  const confirmCancel = document.getElementById('adminConfirmCancel');
  let pendingForm = null;

  function closeConfirmModal() {
    if (confirmBackdrop) {
      confirmBackdrop.setAttribute('hidden', 'hidden');
      pendingForm = null;
    }
  }

  function openConfirmModal(message, form) {
    if (!confirmBackdrop || !confirmMessage) {
      form.submit();
      return;
    }
    confirmMessage.textContent = message || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
    confirmBackdrop.removeAttribute('hidden');
    pendingForm = form;
  }

  if (confirmCancel) {
    confirmCancel.addEventListener('click', closeConfirmModal);
  }
  if (confirmBackdrop) {
    confirmBackdrop.addEventListener('click', function (e) {
      if (e.target === confirmBackdrop) {
        closeConfirmModal();
      }
    });
  }
  if (confirmOk) {
    confirmOk.addEventListener('click', function () {
      if (pendingForm) {
        const form = pendingForm;
        closeConfirmModal();
        form.dataset.confirmProcessed = 'true';
        form.requestSubmit ? form.requestSubmit() : form.submit();
      }
    });
  }

  document.querySelectorAll('form[data-confirm]').forEach(function (form) {
    form.addEventListener('submit', function (event) {
      if (form.dataset.confirmProcessed === 'true') {
        form.dataset.confirmProcessed = 'false';
        return;
      }
      event.preventDefault();
      const message = form.getAttribute('data-confirm');
      openConfirmModal(message, form);
    }, { capture: true });
  });
});

