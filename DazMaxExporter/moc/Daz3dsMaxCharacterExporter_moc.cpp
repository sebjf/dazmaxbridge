/****************************************************************************
** Meta object code from reading C++ file 'Daz3dsMaxCharacterExporter.h'
**
** Created: Sat 3. Aug 20:37:56 2013
**      by: The Qt Meta Object Compiler version 63 (Qt 4.8.1)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include "../Daz3dsMaxCharacterExporter.h"
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'Daz3dsMaxCharacterExporter.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 63
#error "This file was generated using the moc from 4.8.1. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
static const uint qt_meta_data_MyDazExporter[] = {

 // content:
       6,       // revision
       0,       // classname
       0,    0, // classinfo
       3,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       0,       // signalCount

 // slots: signature, parameters, type, tag, flags
      23,   15,   14,   14, 0x0a,
      68,   14,   60,   14, 0x0a,
      90,   14,   85,   14, 0x0a,

       0        // eod
};

static const char qt_meta_stringdata_MyDazExporter[] = {
    "MyDazExporter\0\0options\0"
    "getDefaultOptions(DzFileIOSettings*)\0"
    "QString\0getDescription()\0bool\0"
    "isFileExporter()\0"
};

void MyDazExporter::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        Q_ASSERT(staticMetaObject.cast(_o));
        MyDazExporter *_t = static_cast<MyDazExporter *>(_o);
        switch (_id) {
        case 0: _t->getDefaultOptions((*reinterpret_cast< DzFileIOSettings*(*)>(_a[1]))); break;
        case 1: { QString _r = _t->getDescription();
            if (_a[0]) *reinterpret_cast< QString*>(_a[0]) = _r; }  break;
        case 2: { bool _r = _t->isFileExporter();
            if (_a[0]) *reinterpret_cast< bool*>(_a[0]) = _r; }  break;
        default: ;
        }
    }
}

const QMetaObjectExtraData MyDazExporter::staticMetaObjectExtraData = {
    0,  qt_static_metacall 
};

const QMetaObject MyDazExporter::staticMetaObject = {
    { &DzExporter::staticMetaObject, qt_meta_stringdata_MyDazExporter,
      qt_meta_data_MyDazExporter, &staticMetaObjectExtraData }
};

#ifdef Q_NO_DATA_RELOCATION
const QMetaObject &MyDazExporter::getStaticMetaObject() { return staticMetaObject; }
#endif //Q_NO_DATA_RELOCATION

const QMetaObject *MyDazExporter::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->metaObject : &staticMetaObject;
}

void *MyDazExporter::qt_metacast(const char *_clname)
{
    if (!_clname) return 0;
    if (!strcmp(_clname, qt_meta_stringdata_MyDazExporter))
        return static_cast<void*>(const_cast< MyDazExporter*>(this));
    return DzExporter::qt_metacast(_clname);
}

int MyDazExporter::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = DzExporter::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 3)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 3;
    }
    return _id;
}
QT_END_MOC_NAMESPACE
