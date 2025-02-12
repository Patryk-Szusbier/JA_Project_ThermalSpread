.data
saved_state_xmm6    oword ?,?,?,?,?,?,?,?        ; Rezerwacja pami�ci na zapis stanu rejestru XMM6
saved_state_xmm7    oword ?,?,?,?,?,?,?,?        ; Rezerwacja pami�ci na zapis stanu rejestru XMM7

DefaultPermMaskValue    db 0,   1,  2,  3,  4,  5,  6,  7 
                        db 8,   9, 10, 11, 12, 13, 14, 15 
                        db 16, 17, 18, 19, 20, 21, 22, 23 
                        db 24, 25, 26, 27, 28, 29, 30, 31 
                        db 32, 33, 34, 35, 36, 37, 38, 39 
                        db 40, 41, 42, 43, 44, 45, 46, 47 
                        db 48, 49, 50, 51, 52, 53, 54, 55 
                        db 56, 57, 58, 59, 60, 61, 62, 63
                        ; Tablica mask permutacji - ka�da warto�� odpowiada indeksowi przesuni�cia w tablicy.

.code
_DllMainCRTStartup proc parameter1:DWORD, parameter2:DWORD, parameter3:DWORD
    ; G��wna funkcja inicjalizacyjna DLL
    mov eax, 1                ; Zwr�cenie warto�ci 1 jako sukcesu inicjalizacji
    ret                       ; Powr�t z funkcji
_DllMainCRTStartup endp

;constants
TRUE EQU 1                    ; Definicja sta�ej logicznej TRUE
FALSE EQU 0                   ; Definicja sta�ej logicznej FALSE
COLUMN_INCREMENT EQU 62       ; Przyrost kolumn podczas iteracji

;registers aliases - params
WriteDataAddress EQU RSI      ; Alias dla rejestru RSI, kt�ry przechowuje adres zapisu danych
WidthPx EQU R8                ; Alias dla rejestru R8 przechowuj�cego szeroko�� w pikselach
WidthPx32 EQU R8D             ; Alias dla 32-bitowego R8D do przechowywania szeroko�ci w pikselach
HeightPx EQU R9               ; Alias dla rejestru R9 przechowuj�cego wysoko�� w pikselach

;register variables
IsFinished EQU RAX            ; Rejestr przechowuj�cy informacj� o zako�czeniu oblicze�

UtilityRegister EQU RCX       ; Rejestr pomocniczy
UtilityRegister8 EQU CL       ; Ni�szy bajt rejestru pomocniczego

PointersResetRegister EQU RDX ; Rejestr do resetowania wska�nik�w

RowsIndex EQU R10             ; Rejestr indeksu wierszy
ColumnsIndex EQU R11          ; Rejestr indeksu kolumn
ReadDataAddress EQU R12       ; Rejestr przechowuj�cy adres odczytu danych
FalseRegister EQU R13         ; Rejestr przechowuj�cy warto�� FALSE
UtilityRegister2 EQU R14      ; Drugi rejestr pomocniczy

TopRow EQU ZMM16              ; Rejestr ZMM przechowuj�cy g�rny wiersz macierzy
CenterRow EQU ZMM17           ; Rejestr ZMM przechowuj�cy �rodkowy wiersz macierzy
BottomRow EQU ZMM18           ; Rejestr ZMM przechowuj�cy dolny wiersz macierzy
IntermediateRegister EQU ZMM19 ; Rejestr ZMM do oblicze� po�rednich
MemoryWriteBuffer EQU ZMM20   ; Bufor pami�ci do zapisu wynik�w
WorkingRegister EQU ZMM21     ; Rejestr ZMM roboczy
DefaultPermMask EQU ZMM22     ; Rejestr ZMM przechowuj�cy domy�ln� mask� permutacji
UtilityZMMRegister EQU ZMM23  ; Rejestr ZMM pomocniczy

ReadWriteMemoryMask EQU k0    ; Maskowanie pami�ci dla odczytu/zapisu
ReadWriteMemoryMaskBraces EQU {k0} ; Sk�adnia nawiasowa maskowania dla k0

UtilityMask EQU k1            ; Maska pomocnicza
UtilityMaskBraces EQU {k1}    ; Sk�adnia nawiasowa dla k1

ABCMask EQU k2                ; Maska A/B/C dla wierszy
ABCMaskBraces EQU {k2}        ; Sk�adnia nawiasowa dla k2

DMask EQU k3                  ; Maska dla g�rnego wiersza
DMaskBraces EQU {k3}          ; Sk�adnia nawiasowa dla k3

EMask EQU k4                  ; Maska dla dolnego wiersza
EMaskBraces  EQU {k4}         ; Sk�adnia nawiasowa dla k4

FrameMask EQU k5              ; Maska dla ramki
FrameMaskBraces  EQU {k5}     ; Sk�adnia nawiasowa dla k5

OneByteMask EQU k6            ; Maska dla pojedynczego bajtu
OneByteMaskBraces  EQU {k6}   ; Sk�adnia nawiasowa dla k6

;run's single simulation step for every n'th byte of byte's octet in single row
execute macro N: REQ
    ; Makro wykonuj�ce pojedynczy krok symulacji dla ka�dego bajtu w wierszu.
	;cel: na pierwsze 3 bajty wsadzi� pierwsze 3 bajty Center | reszt� wyzerowa�
	;		na 4 bajt wsadzi� 2 bajt top
	;		na 5 bajt wsadzi� 2 bajt bottom
	
	;CENTER
	;maska na 3 pierwsze bajty z flag� {z}, permutacja taka �eby przesuni�cie by�o 0
	;maska: ABCMask | flaga {z} | permutacja: default ale bajtowo dodaj 0
	MOV UtilityRegister, -N								;tu powinni�my dodawa� -N+1 (ujemna warto�� + 1)
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodali�my 0 do oryginalnych masek
	VPERMB WorkingRegister ABCMaskBraces{z}, UtilityZMMRegister, CenterRow


	;TOP
	MOV UtilityRegister, 3-N								;tu powinni�my dodawa� 3-N (ujemna warto�� + 1 - 2) 2-N+1
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodali�my 3 do oryginalnych masek
	VPERMB WorkingRegister DMaskBraces, UtilityZMMRegister, TopRow

	;BOTTOM
	MOV UtilityRegister, 4-N								;tu powinni�my dodawa� -N+1-3 (ujemna warto�� + 1 - 2)
	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister				;dodali�my 3 do oryginalnych masek
	VPERMB WorkingRegister EMaskBraces, UtilityZMMRegister, BottomRow

	;;;;;tu mamy ju� dobrze wpisane warto�ci, teraz obliczenia:
	VPXORD IntermediateRegister, IntermediateRegister, IntermediateRegister			;clearing register (XOR)
	VPSADBW WorkingRegister, WorkingRegister, IntermediateRegister					;summing octets

	movzx UtilityRegister, Multiplicator
	VPBROADCASTQ UtilityZMMRegister, UtilityRegister
	VPMULDQ WorkingRegister, WorkingRegister, UtilityZMMRegister							;multiplying

	movzx UtilityRegister, Shift
	VPBROADCASTQ UtilityZMMRegister, UtilityRegister	
	VPSRLVQ  WorkingRegister, WorkingRegister, UtilityZMMRegister									;shifting right

	;;;;;tu mamy ju� dobre warto�ci jako QWORDY (w rzeczywisto�ci BYTE), teraz trzeba je wpisa� do Buffer na pozycji N
	;;;;;czyli VPERMB MemoryWriteBuffer, MASKA_N, WorkingRegister, MASK_VPERMB(8->N)
	;;;;; a mo�e przesuni�cie bajtowe o sta�� - nie musi by� CROSS LANE!!! | a mo�e zamiast maski mog� zrobi� OR?
	;przesu� w lewo o 8-N

	;KSHIFTLB UtilityMask, OneByteMask, 7-N
	;KANDB UtilityMask, UtilityMask, FrameMask

	;VPSLLQ WorkingRegister, WorkingRegister, 7-N
;	MOV UtilityRegister, 7-N						
;	VPBROADCASTB UtilityZMMRegister, UtilityRegister		
;	VPADDSB UtilityZMMRegister, DefaultPermMask, UtilityZMMRegister	
;	VPERMB WorkingRegister, UtilityZMMRegister, WorkingRegister

	;VPORQ MemoryWriteBuffer, MemoryWriteBuffer, WorkingRegister
	;tu trzeba wylicza� jak�� wsp�ln� mask� (OR), tak �e 1 jest na odpowiednim bicie w oktecie

	VPSLLDQ WorkingRegister, WorkingRegister, 7-N
	VPORQ MemoryWriteBuffer, MemoryWriteBuffer, WorkingRegister
endm

;run's single simulation step in provided submatrix
_runSingleStep proc EXPORT USES WriteDataAddress ReadDataAddress FalseRegister UtilityRegister2, readData_:PTR BYTE, writeData_:PTR BYTE, WidthPx_:DWORD, HeightPx_:DWORD, StartColumn:QWORD, EndColumn:QWORD, Multiplicator: WORD, Shift: WORD
    ; Funkcja wykonuj�ca symulacj� na podmacierzy.
	;initializing
	    ; Wczytaj domy�ln� mask� permutacji do rejestru ZMM
    VMOVDQU8 DefaultPermMask, DefaultPermMaskValue

    ; Ustaw mask� ABC (0xE0E0...) w rejestrze k2
    mov RAX, 0E0E0E0E0E0E0E0E0h
    kmovq ABCMask, RAX

    ; Ustaw mask� D (0x1010...) w rejestrze k3
    mov RAX, 1010101010101010h
    kmovq DMask, RAX

    ; Ustaw mask� E (0x0808...) w rejestrze k4
    mov RAX, 0808080808080808h
    kmovq EMask, RAX

    ; Ustaw mask� ramki (7FFFFFFFFFFFFFFE) w rejestrze k5
    mov RAX, 7FFFFFFFFFFFFFFEh
    kmovq FrameMask, RAX

    ; Ustaw mask� dla jednego bajtu (0x010101...) w rejestrze k6
    mov RAX, 0101010101010101h
    kmovq OneByteMask, RAX

    ; Oblicz przesuni�cie wska�nika (wysoko�� * szeroko�� w pikselach)
    mov RAX, HeightPx
    mul WidthPx32
    mov PointersResetRegister, RAX

    ; Zainicjalizuj rejestr jako FALSE
    mov FalseRegister, FALSE

    ; Zainicjalizuj rejestr jako TRUE (algorytm zako�czony domy�lnie)
    mov IsFinished, TRUE

    ; Rozpocznij iteracj� po kolumnach (ColumnsIndex = StartColumn)
    mov ColumnsIndex, StartColumn

columns_loop:
    ; Oblicz mask� do wczytywania/zapisywania pami�ci (ignoruj bajty poza granicami)
    mov UtilityRegister2, 0FFFFFFFFFFFFFFFFh    ; Maska pe�na (wszystkie bity na 1)
    mov UtilityRegister, EndColumn             ; Za�aduj ostatni� kolumn�
    sub UtilityRegister, ColumnsIndex          ; Oblicz r�nic� mi�dzy ko�cem a bie��c� kolumn�
    sub UtilityRegister, 64                    ; Odejmij rozmiar (64 bajty)
    neg UtilityRegister                        ; Negacja wyniku
    test UtilityRegister, 0                    ; Sprawd�, czy wynik jest wi�kszy od 0
    cmovnbe UtilityRegister, FalseRegister     ; Je�li nie, ustaw na FALSE
    shl UtilityRegister2, UtilityRegister8     ; Przesu� mask� w lewo o obliczon� warto��
    kmovq ReadWriteMemoryMask, UtilityRegister2 ; Przypisz wynikow� mask�

    ; Za�aduj g�rny wiersz z pami�ci do rejestru ZMM (z mask�)
    VMOVDQU8 TopRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress]

    ; Przesu� wska�nik pami�ci do kolejnego wiersza
    add ReadDataAddress, WidthPx
    add WriteDataAddress, WidthPx

    ; Za�aduj �rodkowy wiersz
    VMOVDQU8 CenterRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress]

    ; Zainicjalizuj indeks wierszy na 1
    mov RowsIndex, 1

rows_loop:
    ; Za�aduj dolny wiersz z pami�ci
    VMOVDQU8 BottomRow ReadWriteMemoryMaskBraces{z}, [ReadDataAddress + WidthPx]

    ; Wyzeruj bufor pami�ci (czyszczenie rejestru)
    VPXORD MemoryWriteBuffer, MemoryWriteBuffer, MemoryWriteBuffer

    ; Wykonaj obliczenia dla ka�dego bajtu wiersza
    execute 0
    execute 1
    execute 2
    execute 3
    execute 4
    execute 5
    execute 6
    execute 7

    ; Sprawd�, czy dowolny bajt w buforze jest > 0 (algorytm niesko�czony)
    VPTESTMQ UtilityMask, MemoryWriteBuffer, MemoryWriteBuffer
    KORTESTB UtilityMask, UtilityMask
    cmovnz IsFinished, FalseRegister

    ; Przesu� wiersze w d�
    VMOVDQU64 TopRow, CenterRow
    VMOVDQU64 CenterRow, BottomRow

    ; Zapisz bufor wynikowy do pami�ci
    VMOVDQU8 [WriteDataAddress] ReadWriteMemoryMaskBraces, MemoryWriteBuffer

    ; Przesu� wska�niki pami�ci do nast�pnego wiersza
    add ReadDataAddress, WidthPx
    add WriteDataAddress, WidthPx

    ; Zwi�ksz indeks wierszy
    inc RowsIndex
    cmp RowsIndex, HeightPx
    jna rows_loop

    ; Zresetuj wska�niki wierszy
    sub ReadDataAddress, PointersResetRegister
    sub WriteDataAddress, PointersResetRegister

    ; Przesu� wska�niki kolumn o sta�� warto��
    add ReadDataAddress, COLUMN_INCREMENT
    add WriteDataAddress, COLUMN_INCREMENT

    ; Zwi�ksz indeks kolumn
    add ColumnsIndex, COLUMN_INCREMENT
    cmp ColumnsIndex, EndColumn
    jna columns_loop

    ; Zako�cz procedur�
    ret
_runSingleStep endp

END