.data
saved_state_xmm6    oword ?,?,?,?,?,?,?,?        ; Rezerwacja pamiêci na zapis stanu rejestru XMM6
saved_state_xmm7    oword ?,?,?,?,?,?,?,?        ; Rezerwacja pamiêci na zapis stanu rejestru XMM7

DefaultPermMaskValue    db 0,   1,  2,  3,  4,  5,  6,  7 
                        db 8,   9, 10, 11, 12, 13, 14, 15 
                        db 16, 17, 18, 19, 20, 21, 22, 23 
                        db 24, 25, 26, 27, 28, 29, 30, 31 
                        db 32, 33, 34, 35, 36, 37, 38, 39 
                        db 40, 41, 42, 43, 44, 45, 46, 47 
                        db 48, 49, 50, 51, 52, 53, 54, 55 
                        db 56, 57, 58, 59, 60, 61, 62, 63
                        ; Tablica mask permutacji - ka¿da wartoœæ odpowiada indeksowi przesuniêcia w tablicy.

.code
_DllMainCRTStartup proc parameter1:DWORD, parameter2:DWORD, parameter3:DWORD
    ; G³ówna funkcja inicjalizacyjna DLL
    mov eax, 1                ; Zwrócenie wartoœci 1 jako sukcesu inicjalizacji
    ret                       ; Powrót z funkcji
_DllMainCRTStartup endp

;constants
TRUE EQU 1                    ; Definicja sta³ej logicznej TRUE
FALSE EQU 0                   ; Definicja sta³ej logicznej FALSE
COLUMN_INCREMENT EQU 62       ; Przyrost kolumn podczas iteracji

;registers aliases - params
WriteDataAddress EQU RSI      ; Alias dla rejestru RSI, który przechowuje adres zapisu danych
WidthPx EQU R8                ; Alias dla rejestru R8 przechowuj¹cego szerokoœæ w pikselach
WidthPx32 EQU R8D             ; Alias dla 32-bitowego R8D do przechowywania szerokoœci w pikselach
HeightPx EQU R9               ; Alias dla rejestru R9 przechowuj¹cego wysokoœæ w pikselach

;register variables
IsFinished EQU RAX            ; Rejestr przechowuj¹cy informacjê o zakoñczeniu obliczeñ

UtilityRegister EQU RCX       ; Rejestr pomocniczy
UtilityRegister8 EQU CL       ; Ni¿szy bajt rejestru pomocniczego

PointersResetRegister EQU RDX ; Rejestr do resetowania wskaŸników

RowsIndex EQU R10             ; Rejestr indeksu wierszy
ColumnsIndex EQU R11          ; Rejestr indeksu kolumn
ReadDataAddress EQU R12       ; Rejestr przechowuj¹cy adres odczytu danych
FalseRegister EQU R13         ; Rejestr przechowuj¹cy wartoœæ FALSE
UtilityRegister2 EQU R14      ; Drugi rejestr pomocniczy

TopRow EQU ZMM16              ; Rejestr ZMM przechowuj¹cy górny wiersz macierzy
CenterRow EQU ZMM17           ; Rejestr ZMM przechowuj¹cy œrodkowy wiersz macierzy
BottomRow EQU ZMM18           ; Rejestr ZMM przechowuj¹cy dolny wiersz macierzy
IntermediateRegister EQU ZMM19 ; Rejestr ZMM do obliczeñ poœrednich
MemoryWriteBuffer EQU ZMM20   ; Bufor pamiêci do zapisu wyników
WorkingRegister EQU ZMM21     ; Rejestr ZMM roboczy
DefaultPermMask EQU ZMM22     ; Rejestr ZMM przechowuj¹cy domyœln¹ maskê permutacji
UtilityZMMRegister EQU ZMM23  ; Rejestr ZMM pomocniczy

ReadWriteMemoryMask EQU k0    ; Maskowanie pamiêci dla odczytu/zapisu
ReadWriteMemoryMaskBraces EQU {k0} ; Sk³adnia nawiasowa maskowania dla k0

UtilityMask EQU k1            ; Maska pomocnicza
UtilityMaskBraces EQU {k1}    ; Sk³adnia nawiasowa dla k1

ABCMask EQU k2                ; Maska A/B/C dla wierszy
ABCMaskBraces EQU {k2}        ; Sk³adnia nawiasowa dla k2

DMask EQU k3                  ; Maska dla górnego wiersza
DMaskBraces EQU {k3}          ; Sk³adnia nawiasowa dla k3

EMask EQU k4                  ; Maska dla dolnego wiersza
EMaskBraces  EQU {k4}         ; Sk³adnia nawiasowa dla k4

FrameMask EQU k5              ; Maska dla ramki
FrameMaskBraces  EQU {k5}     ; Sk³adnia nawiasowa dla k5

OneByteMask EQU k6            ; Maska dla pojedynczego bajtu
OneByteMaskBraces  EQU {k6}   ; Sk³adnia nawiasowa dla k6

;run's single simulation step for every n'th byte of byte's octet in single row
execute macro N: REQ
    ; Makro wykonuj¹ce pojedynczy krok symulacji dla ka¿dego bajtu w wierszu.
	;cel: na pierwsze 3 bajty wsadziæ pierwsze 3 bajty Center | resztê wyzerowaæ
	;		na 4 bajt wsadziæ 2 bajt top
	;		na 5 bajt wsadziæ 2 bajt bottom
	
	;CENTER
	;maska na 3 pierwsze bajty z flag¹ {z}, permutacja taka ¿eby przesuniêcie by³o 0
	;maska: ABCMask | flaga {z} | permutacja: default ale bajtowo dodaj 0
	MOV UtilityRegister, -N								;tu powinniœmy dodawaæ -N+1 (ujemna wartoœæ + 1)
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodaliœmy 0 do oryginalnych masek
	VPERMB WorkingRegister ABCMaskBraces{z}, UtilityZMMRegister, CenterRow


	;TOP
	MOV UtilityRegister, 3-N								;tu powinniœmy dodawaæ 3-N (ujemna wartoœæ + 1 - 2) 2-N+1
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodaliœmy 3 do oryginalnych masek
	VPERMB WorkingRegister DMaskBraces, UtilityZMMRegister, TopRow

	;BOTTOM
	MOV UtilityRegister, 4-N								;tu powinniœmy dodawaæ -N+1-3 (ujemna wartoœæ + 1 - 2)
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodaliœmy 3 do oryginalnych masek
	VPERMB WorkingRegister EMaskBraces, UtilityZMMRegister, BottomRow

	;;;;;tu mamy ju¿ dobrze wpisane wartoœci, teraz obliczenia:
	VPXORD IntermediateRegister, IntermediateRegister, IntermediateRegister			;clearing register (XOR)
	VPSADBW WorkingRegister, WorkingRegister, IntermediateRegister					;summing octets

	movzx UtilityRegister, Multiplicator
	VPBROADCASTQ UtilityZMMRegister, UtilityRegister
	VPMULDQ WorkingRegister, WorkingRegister, UtilityZMMRegister							;multiplying

	movzx UtilityRegister, Shift
	VPBROADCASTQ UtilityZMMRegister, UtilityRegister	
	VPSRLVQ  WorkingRegister, WorkingRegister, UtilityZMMRegister									;shifting right

	;;;;;tu mamy ju¿ dobre wartoœci jako QWORDY (w rzeczywistoœci BYTE), teraz trzeba je wpisaæ do Buffer na pozycji N
	;;;;;czyli VPERMB MemoryWriteBuffer, MASKA_N, WorkingRegister, MASK_VPERMB(8->N)
	;;;;; a mo¿e przesuniêcie bajtowe o sta³¹ - nie musi byæ CROSS LANE!!! | a mo¿e zamiast maski mogê zrobiæ OR?
	;przesuñ w lewo o 8-N

	;KSHIFTLB UtilityMask, OneByteMask, 7-N
	;KANDB UtilityMask, UtilityMask, FrameMask

	;VPSLLQ WorkingRegister, WorkingRegister, 7-N
;	MOV UtilityRegister, 7-N						
;	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
;	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister	
;	VPERMB WorkingRegister, UtilityZMMRegister, WorkingRegister

	;VPORQ MemoryWriteBuffer, MemoryWriteBuffer, WorkingRegister
	;tu trzeba wyliczaæ jak¹œ wspóln¹ maskê (OR), tak ¿e 1 jest na odpowiednim bicie w oktecie

	VPSLLDQ WorkingRegister, WorkingRegister, 7-N
	VPORQ MemoryWriteBuffer, MemoryWriteBuffer, WorkingRegister
endm

;run's single simulation step in provided submatrix
_runSingleStep proc EXPORT USES WriteDataAddress ReadDataAddress FalseRegister UtilityRegister2, readData_:PTR BYTE, writeData_:PTR BYTE, WidthPx_:DWORD, HeightPx_:DWORD, StartColumn:QWORD, EndColumn:QWORD, Multiplicator: WORD, Shift: WORD
    ; Funkcja wykonuj¹ca symulacjê na podmacierzy.
	;initializing
	    ; Wczytaj domyœln¹ maskê permutacji do rejestru ZMM
    VMOVDQU8 DefaultPermMask, DefaultPermMaskValue

    ; Ustaw maskê ABC (0xE0E0...) w rejestrze k2
    mov RAX, 0E0E0E0E0E0E0E0E0h
    kmovq ABCMask, RAX

    ; Ustaw maskê D (0x1010...) w rejestrze k3
    mov RAX, 1010101010101010h
    kmovq DMask, RAX

    ; Ustaw maskê E (0x0808...) w rejestrze k4
    mov RAX, 0808080808080808h
    kmovq EMask, RAX

    ; Ustaw maskê ramki (7FFFFFFFFFFFFFFE) w rejestrze k5
    mov RAX, 7FFFFFFFFFFFFFFEh
    kmovq FrameMask, RAX

    ; Ustaw maskê dla jednego bajtu (0x010101...) w rejestrze k6
    mov RAX, 0101010101010101h
    kmovq OneByteMask, RAX

    ; Oblicz przesuniêcie wskaŸnika (wysokoœæ * szerokoœæ w pikselach)
    mov RAX, HeightPx
    mul WidthPx32
    mov PointersResetRegister, RAX

    ; Zainicjalizuj rejestr jako FALSE
    mov FalseRegister, FALSE

    ; Zainicjalizuj rejestr jako TRUE (algorytm zakoñczony domyœlnie)
    mov IsFinished, TRUE

    ; Rozpocznij iteracjê po kolumnach (ColumnsIndex = StartColumn)
    mov ColumnsIndex, StartColumn

columns_loop:
    ; Oblicz maskê do wczytywania/zapisywania pamiêci (ignoruj bajty poza granicami)
    mov UtilityRegister2, 0FFFFFFFFFFFFFFFFh    ; Maska pe³na (wszystkie bity na 1)
    mov UtilityRegister, EndColumn             ; Za³aduj ostatni¹ kolumnê
    sub UtilityRegister, ColumnsIndex          ; Oblicz ró¿nicê miêdzy koñcem a bie¿¹c¹ kolumn¹
    sub UtilityRegister, 64                    ; Odejmij rozmiar (64 bajty)
    neg UtilityRegister                        ; Negacja wyniku
    test UtilityRegister, 0                    ; SprawdŸ, czy wynik jest wiêkszy od 0
    cmovnbe UtilityRegister, FalseRegister     ; Jeœli nie, ustaw na FALSE
    shl UtilityRegister2, UtilityRegister8     ; Przesuñ maskê w lewo o obliczon¹ wartoœæ
    kmovq ReadWriteMemoryMask, UtilityRegister2 ; Przypisz wynikow¹ maskê

    ; Za³aduj górny wiersz z pamiêci do rejestru ZMM (z mask¹)
    VMOVDQU8 TopRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress]

    ; Przesuñ wskaŸnik pamiêci do kolejnego wiersza
    add ReadDataAddress, WidthPx
    add WriteDataAddress, WidthPx

    ; Za³aduj œrodkowy wiersz
    VMOVDQU8 CenterRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress]

    ; Zainicjalizuj indeks wierszy na 1
    mov RowsIndex, 1

rows_loop:
    ; Za³aduj dolny wiersz z pamiêci
    VMOVDQU8 BottomRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress + WidthPx]

    ; Wyzeruj bufor pamiêci (czyszczenie rejestru)
    VPXORD MemoryWriteBuffer, MemoryWriteBuffer, MemoryWriteBuffer

    ; Wykonaj obliczenia dla ka¿dego bajtu wiersza
    execute 0
    execute 1
    execute 2
    execute 3
    execute 4
    execute 5
    execute 6
    execute 7

    ; SprawdŸ, czy dowolny bajt w buforze jest > 0 (algorytm nieskoñczony)
    VPTESTMQ UtilityMask, MemoryWriteBuffer, MemoryWriteBuffer
    KORTESTB UtilityMask, UtilityMask
    cmovnz IsFinished, FalseRegister

    ; Przesuñ wiersze w dó³
    VMOVDQU64 TopRow, CenterRow
    VMOVDQU64 CenterRow, BottomRow

    ; Zapisz bufor wynikowy do pamiêci
    VMOVDQU8 [WriteDataAddress] ReadWriteMemoryMaskBraces, MemoryWriteBuffer

    ; Przesuñ wskaŸniki pamiêci do nastêpnego wiersza
    add ReadDataAddress, WidthPx
    add WriteDataAddress, WidthPx

    ; Zwiêksz indeks wierszy
    inc RowsIndex
    cmp RowsIndex, HeightPx
    jna rows_loop

    ; Zresetuj wskaŸniki wierszy
    sub ReadDataAddress, PointersResetRegister
    sub WriteDataAddress, PointersResetRegister

    ; Przesuñ wskaŸniki kolumn o sta³¹ wartoœæ
    add ReadDataAddress, COLUMN_INCREMENT
    add WriteDataAddress, COLUMN_INCREMENT

    ; Zwiêksz indeks kolumn
    add ColumnsIndex, COLUMN_INCREMENT
    cmp ColumnsIndex, EndColumn
    jna columns_loop

    ; Zakoñcz procedurê
    ret
_runSingleStep endp

END