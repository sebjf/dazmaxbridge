/****************************************************************************
** Meta object code from reading C++ file 'SceneServer.h'
**
** Created: Tue 20. Aug 22:59:30 2013
**      by: The Qt Meta Object Compiler version 63 (Qt 4.8.1)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include "../SceneServer.h"
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'SceneServer.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 63
#error "This file was generated using the moc from 4.8.1. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
static const uint qt_meta_data_MySceneServer[] = {

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
      15,   14,   14,   14, 0x0a,
      34,   14,   14,   14, 0x0a,
      49,   14,   14,   14, 0x0a,

       0        // eod
};

static const char qt_meta_stringdata_MySceneServer[] = {
    "MySceneServer\0\0on_newConnection()\0"
    "on_readyRead()\0on_disconnected()\0"
};

void MySceneServer::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        Q_ASSERT(staticMetaObject.cast(_o));
        MySceneServer *_t = static_cast<MySceneServer *>(_o);
        switch (_id) {
        case 0: _t->on_newConnection(); break;
        case 1: _t->on_readyRead(); break;
        case 2: _t->on_disconnected(); break;
        default: ;
        }
    }
    Q_UNUSED(_a);
}

const QMetaObjectExtraData MySceneServer::staticMetaObjectExtraData = {
    0,  qt_static_metacall 
};

const QMetaObject MySceneServer::staticMetaObject = {
    { &QObject::staticMetaObject, qt_meta_stringdata_MySceneServer,
      qt_meta_data_MySceneServer, &staticMetaObjectExtraData }
};

#ifdef Q_NO_DATA_RELOCATION
const QMetaObject &MySceneServer::getStaticMetaObject() { return staticMetaObject; }
#endif //Q_NO_DATA_RELOCATION

const QMetaObject *MySceneServer::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->metaObject : &staticMetaObject;
}

void *MySceneServer::qt_metacast(const char *_clname)
{
    if (!_clname) return 0;
    if (!strcmp(_clname, qt_meta_stringdata_MySceneServer))
        return static_cast<void*>(const_cast< MySceneServer*>(this));
    return QObject::qt_metacast(_clname);
}

int MySceneServer::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QObject::qt_metacall(_c, _id, _a);
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
