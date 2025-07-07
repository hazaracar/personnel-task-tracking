document.addEventListener("DOMContentLoaded", function () {
    const dropdownTrigger = document.getElementById('notificationDropdown');
    const dropdownMenu = document.getElementById('notificationList');

    // Bildirimleri AJAX ile getir
    $.get('/Notification/GetLatest', function (data) {
        const unreadCount = data.filter(n => !n.isRead).length;
        $('#unreadCount').text(unreadCount > 0 ? unreadCount : '');

        if (data.length > 0) {
            let items = '<li class="dropdown-header fw-bold">Bildirimler</li>';

            data.forEach(item => {
                const isReadClass = item.isRead ? 'fw-normal' : 'fw-bold bg-light';
                const iconClass = 'fa fa-bell text-theme mt-1';

                items += `
                    <li>
                        <div class="dropdown-item notification-item d-flex justify-content-between ${isReadClass}" data-id="${item.id}">
                            <div class="d-flex align-items-start gap-2 flex-grow-1 me-2">
                                <i class="${iconClass} mt-1"></i>
                                <div class="notification-content">
                                    <a href="#" onclick="markAsRead(event, ${item.id}, '${item.link || '#'}')" class="text-decoration-none text-dark d-block">
                                        <div class="fw-semibold notification-message">${item.message}</div>
                                        <small class="text-muted d-block">${formatDate(item.createdAt)}</small>
                                    </a>
                                </div>
                            </div>
                            <button class="btn btn-sm btn-link text-danger p-0 flex-shrink-0 delete-btn" onclick="deleteNotif(event, ${item.id})" title="Sil">
                                <i class="fa fa-trash"></i>
                            </button>
                        </div>
                    </li>`;


            });

            $('#notificationList').html(items);
        } else {
            $('#notificationList').html('<li class="dropdown-item text-muted text-center small">Yeni bildirim yok</li>');
        }
    });

    // Dropdown toggle
    if (dropdownTrigger && dropdownMenu) {
        dropdownTrigger.addEventListener('click', function (e) {
            e.preventDefault();
            const isShown = dropdownMenu.classList.contains('show');
            dropdownMenu.classList.toggle('show', !isShown);
            dropdownTrigger.setAttribute('aria-expanded', !isShown);
        });

        document.addEventListener('click', function (event) {
            if (!dropdownTrigger.contains(event.target) && !dropdownMenu.contains(event.target)) {
                dropdownMenu.classList.remove('show');
                dropdownTrigger.setAttribute('aria-expanded', 'false');
            }
        });
    }

    // Tarih formatlayıcı
    function formatDate(dateString) {
        const date = new Date(dateString);
        const gun = String(date.getDate()).padStart(2, '0');
        const ay = String(date.getMonth() + 1).padStart(2, '0');
        const yil = date.getFullYear();
        const saat = String(date.getHours()).padStart(2, '0');
        const dk = String(date.getMinutes()).padStart(2, '0');
        return `${gun}.${ay}.${yil} ${saat}:${dk}`;
    }
});

// okundu işaretle ve yönlendir
function markAsRead(event, id, link) {
    event.preventDefault();
    $.post('/Notification/MarkAsRead', { id }, function () {
        const item = $(`[data-id='${id}']`);
        item.removeClass('fw-bold bg-light').addClass('fw-normal');
        const current = parseInt($('#unreadCount').text()) || 0;
        const newCount = Math.max(current - 1, 0);
        $('#unreadCount').text(newCount > 0 ? newCount : '');
        window.location.href = link;
    });
}

//bildirimi sil
function deleteNotif(event, id) {
    event.stopPropagation();
    event.preventDefault();
    $.post('/Notification/Delete', { id }, function () {
        $(`[data-id='${id}']`).closest('li').remove();
        const current = parseInt($('#unreadCount').text()) || 0;
        const newCount = Math.max(current - 1, 0);
        $('#unreadCount').text(newCount > 0 ? newCount : '');
    });
}
