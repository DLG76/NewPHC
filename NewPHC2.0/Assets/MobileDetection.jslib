mergeInto(LibraryManager.library, {
    IsMobileDevice: function () {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;
        
        // ตรวจสอบว่า user agent เป็นอุปกรณ์มือถือ (รวมทั้งโทรศัพท์และแท็บเล็ต)
        if (/android/i.test(userAgent) || /iPhone|iPad|iPod/i.test(userAgent)) {
            return 1; // ถ้าเป็นอุปกรณ์มือถือให้คืนค่า 1
        }
        return 0; // ถ้าไม่ใช่คืนค่า 0
    }
});
