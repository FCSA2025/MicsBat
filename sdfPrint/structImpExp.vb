
Namespace sdfPrint
    Public Structure structSDFband
        Public cmd As String
        Public recstat As String
        Public bndcde As String
        Public bandbitpos As String
        Public blo As String
        Public bmidf As String
        Public bhi As String
        Public badj As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFband)
            cmd = xx.cmd
            recstat = xx.recstat
            bndcde = xx.bndcde
            bandbitpos = xx.bandbitpos
            blo = xx.blo
            bmidf = xx.bmidf
            bhi = xx.bhi
            badj = xx.badj
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub
    End Structure
    Public Structure structSDFeqpt
        Public cmd As String
        Public recstat As String
        Public ecode As String
        Public estab As String
        Public exref As String
        Public emanu As String
        Public emodel As String
        Public edesc As String
        Public etype As String
        Public etraf As String
        Public emission As String
        Public e1stif As String
        Public e2ndif As String
        Public thhold As String
        Public ebndcde As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFeqpt)
            cmd = xx.cmd
            recstat = xx.recstat
            ecode = xx.ecode
            estab = xx.estab
            exref = xx.exref
            emanu = xx.emanu
            emodel = xx.emodel
            edesc = xx.edesc
            etype = xx.etype
            etraf = xx.etraf
            emission = xx.emission
            e1stif = xx.e1stif
            e2ndif = xx.e2ndif
            thhold = xx.thhold
            ebndcde = xx.ebndcde
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub

    End Structure
    Public Structure structSDFnote
        Public cmd As String
        Public recstat As String
        Public oper As String
        Public nonum As String
        Public note As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFnote)
            cmd = xx.cmd
            recstat = xx.recstat
            oper = xx.oper
            nonum = xx.nonum
            note = xx.note
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub

    End Structure
    Public Structure structSDFoper
        Public cmd As String
        Public recstat As String
        Public oper As String
        Public nameop As String
        Public cooper As String
        Public mdbm As String
        Public addr As String
        Public city As String
        Public prstat As String
        Public zippc As String
        Public dept As String
        Public namep As String
        Public phonep As String
        Public faxnum As String
        Public telecom As String
        Public opnote As String
        Public admin As String
        Public email As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFoper)
            cmd = xx.cmd
            recstat = xx.recstat
            oper = xx.oper
            nameop = xx.nameop
            cooper = xx.cooper
            mdbm = xx.mdbm
            addr = xx.addr
            city = xx.city
            prstat = xx.prstat
            zippc = xx.zippc
            dept = xx.dept
            namep = xx.namep
            phonep = xx.phonep
            faxnum = xx.faxnum
            telecom = xx.telecom
            opnote = xx.opnote
            admin = xx.admin
            email = xx.email
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub
    End Structure
    Public Structure structSDFrout
        Public cmd As String
        Public recstat As String
        Public rcomp As String
        Public routnumb As String
        Public rtprov As String
        Public rtcall As String
        Public rtname As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFrout)
            cmd = xx.cmd
            recstat = xx.recstat
            rcomp = xx.rcomp
            routnumb = xx.routnumb
            rtprov = xx.rtprov
            rtcall = xx.rtcall
            rtname = xx.rtname
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub
    End Structure
    Public Structure structSDFtown
        Public cmd As String
        Public recstat As String
        Public call1 As String
        Public oper As String
        Public twcode As String
        Public twht As String
        Public atwrno As String
        Public twli As String
        Public twpa As String
        Public nott As String
        Public tpoint As String
        Public aDay As String
        Public aMonth As String
        Public aYear As String
        Public sDay As String
        Public sMonth As String
        Public sYear As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFtown)
            cmd = xx.cmd
            recstat = xx.cmd
            call1 = xx.call1
            oper = xx.oper
            twcode = xx.twcode
            twht = xx.twht
            atwrno = xx.atwrno
            twli = xx.twli
            twpa = xx.twpa
            nott = xx.nott
            tpoint = xx.tpoint
            aDay = xx.aDay
            aMonth = xx.aMonth
            aYear = xx.aYear
            sDay = xx.sDay
            sMonth = xx.sMonth
            sYear = xx.sYear
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub
    End Structure
    Public Structure structSDFtowr
        Public cmd As String
        Public recstat As String
        Public twcode As String
        Public twdesc As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFtowr)
            cmd = xx.cmd
            recstat = xx.recstat
            twcode = xx.twcode
            twdesc = xx.twdesc
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime
        End Sub
    End Structure
    Public Structure structSDFtraf
        Public cmd As String
        Public recstat As String
        Public trafcode As String
        Public ecode As String
        Public xreftrcde As String
        Public xrefeqcde As String
        Public trdesc As String
        Public mDay As String
        Public mMonth As String
        Public mYear As String
        Public mtime As String

        Public Sub New(ByVal xx As structSDFtraf)
            cmd = xx.cmd
            recstat = xx.recstat
            trafcode = xx.trafcode
            ecode = xx.ecode
            xreftrcde = xx.xreftrcde
            xrefeqcde = xx.xrefeqcde
            trdesc = xx.trdesc
            mDay = xx.mDay
            mMonth = xx.mMonth
            mYear = xx.mYear
            mtime = xx.mtime

        End Sub

    End Structure

End Namespace
