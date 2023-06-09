use orchard::{
    keys::{DiversifierIndex, FullViewingKey, IncomingViewingKey, Scope, SpendingKey},
    Address,
};

#[no_mangle]
pub extern "C" fn decrypt_orchard_diversifier(
    ivk: *const [u8; 64],
    receiver: *const [u8; 43],
    diversifier_index: *mut [u8; 11],
) -> i32 {
    let ivk = unsafe { &*ivk };
    let receiver = unsafe { &*receiver };
    let ivk = IncomingViewingKey::from_bytes(ivk);
    if ivk.is_none().into() {
        return -1;
    }

    let ivk = ivk.unwrap();
    let address = Address::from_raw_address_bytes(receiver);
    if address.is_none().into() {
        return -2;
    }

    let address = address.unwrap();
    if let Some(index) = ivk.diversifier_index(&address) {
        let diversifier = unsafe { &mut *diversifier_index };
        diversifier.copy_from_slice(index.to_bytes());
        0
    } else {
        1
    }
}

#[no_mangle]
pub extern "C" fn get_orchard_ivk_from_fvk(fvk: *const [u8; 96], ivk: *mut [u8; 64]) -> i32 {
    let fvk = unsafe { &*fvk };
    let ivk = unsafe { &mut *ivk };
    let fvk = FullViewingKey::from_bytes(fvk);
    if fvk.is_none().into() {
        return -1;
    }

    let fvk = fvk.unwrap();
    let external_ivk = fvk.to_ivk(Scope::External);
    ivk.copy_from_slice(&external_ivk.to_bytes());
    0
}

fn get_fvk_from_spending_key(spending_key: &[u8; 32]) -> Option<[u8; 96]> {
    let sk = SpendingKey::from_bytes(*spending_key);
    match sk.is_some().into() {
        true => {
            let sk = sk.unwrap();
            Some(FullViewingKey::from(&sk).to_bytes())
        }
        false => None,
    }
}

#[no_mangle]
pub extern "C" fn get_orchard_fvk_bytes_from_sk_bytes(
    spending_key: *const [u8; 32],
    fvk: *mut [u8; 96],
) -> i32 {
    let spending_key = unsafe { &*spending_key };
    let fvk = unsafe { &mut *fvk };

    match get_fvk_from_spending_key(spending_key) {
        Some(fvk_bytes) => {
            fvk.copy_from_slice(&fvk_bytes);
            0
        }
        None => -1,
    }
}

fn get_raw_payment_address_from_fvk(
    fvk: &[u8; 96],
    diversifier_index: impl Into<DiversifierIndex>,
) -> Option<[u8; 43]> {
    let fvk = FullViewingKey::from_bytes(fvk);
    match fvk.is_some().into() {
        true => {
            let fvk = fvk.unwrap();
            Some(
                fvk.address_at(diversifier_index, Scope::External)
                    .to_raw_address_bytes(),
            )
        }
        false => None,
    }
}

#[no_mangle]
pub extern "C" fn get_orchard_raw_payment_address_from_fvk(
    fvk: *const [u8; 96],
    diversifier_index: *const [u8; 11],
    raw_payment_address: *mut [u8; 43],
) -> i32 {
    let fvk = unsafe { &*fvk };
    let diversifier_index = unsafe { &*diversifier_index };
    let raw_payment_address = unsafe { &mut *raw_payment_address };

    match get_raw_payment_address_from_fvk(fvk, diversifier_index.clone()) {
        Some(raw_payment_address_bytes) => {
            raw_payment_address.copy_from_slice(&raw_payment_address_bytes);
            0
        }
        None => -1,
    }
}
