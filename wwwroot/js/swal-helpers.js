





// Başarı mesajı
function showSuccess(message) {
    Swal.fire({
        icon: 'success',
        title: 'Başarılı!',
        text: message,
        timer: 2000,
        showConfirmButton: false
    });
}

//Hata mesajı
function showError(message) {
    Swal.fire({
        icon: 'error',
        title: 'Hata!',
        text: message
    });
}

// Onay kutusu
function showConfirm(message, confirmCallback) {
    Swal.fire({
        title: 'Emin misiniz?',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet',
        cancelButtonText: 'Vazgeç',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            confirmCallback();
        }
    });
}

// Yükleniyor göstergesi 
function showLoading(message = 'Lütfen bekleyin...') {
    Swal.fire({
        title: message,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

//Yükleme sonrası mesaj
function closeLoadingWithSuccess(message) {
    Swal.fire({
        icon: 'success',
        title: 'Tamamlandı!',
        text: message,
        timer: 2000,
        showConfirmButton: false
    });
}
